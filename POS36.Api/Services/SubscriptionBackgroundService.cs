using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using Serilog;

namespace POS36.Api.Services
{
    /// <summary>
    /// Background Service chạy mỗi giờ:
    /// - Cửa hàng hết hạn → chuyển sang "ChiDoc" (Read-only 15 ngày)
    /// - Quá 15 ngày read-only → "BiKhoa" (Khóa hoàn toàn)
    /// </summary>
    public class SubscriptionBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public SubscriptionBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("⏰ SubscriptionBackgroundService đã khởi động.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var now = DateTime.Now;

                    // 1. Cửa hàng đang HoatDong hoặc DungThu nhưng đã HẾT HẠN → ChiDoc
                    var hetHan = await context.CuaHangs
                        .Where(c => (c.TrangThai == "HoatDong" || c.TrangThai == "DungThu")
                                    && c.NgayHetHan < now)
                        .ToListAsync(stoppingToken);

                    foreach (var store in hetHan)
                    {
                        store.TrangThai = "ChiDoc";
                        Log.Warning("📖 Cửa hàng [{Id}] {TenCuaHang} → Chuyển sang CHỈ ĐỌC (hết hạn).",
                            store.Id, store.TenCuaHang);
                    }

                    // 2. Cửa hàng ChiDoc quá 15 ngày → BiKhoa
                    var quaHan15Ngay = await context.CuaHangs
                        .Where(c => c.TrangThai == "ChiDoc"
                                    && c.NgayHetHan.AddDays(15) < now)
                        .ToListAsync(stoppingToken);

                    foreach (var store in quaHan15Ngay)
                    {
                        store.TrangThai = "BiKhoa";
                        Log.Warning("🔒 Cửa hàng [{Id}] {TenCuaHang} → BỊ KHÓA (quá 15 ngày không gia hạn).",
                            store.Id, store.TenCuaHang);
                    }

                    if (hetHan.Count > 0 || quaHan15Ngay.Count > 0)
                    {
                        await context.SaveChangesAsync(stoppingToken);
                        Log.Information("⏰ Kiểm tra subscription: {HetHan} hết hạn, {BiKhoa} bị khóa.",
                            hetHan.Count, quaHan15Ngay.Count);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "❌ Lỗi SubscriptionBackgroundService");
                }

                // Chạy lại mỗi 1 giờ
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
