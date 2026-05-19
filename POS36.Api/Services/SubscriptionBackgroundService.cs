using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using POS36.Api.Models;
using Serilog;

namespace POS36.Api.Services
{
    /// <summary>
    /// Background Service chạy đúng 00:00:01 mỗi ngày:
    /// - HoatDong/DungThu hết hạn → ChiDoc (15 ngày grace period)
    /// - ChiDoc quá 15 ngày → BiKhoa
    /// - Ghi NhatKyHeThong cho mọi thay đổi tự động
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
            Log.Information("⏰ SubscriptionBackgroundService đã khởi động — chờ đến 00:00:01...");

            // Chạy ngay lần đầu khi khởi động (check những quán đã hết hạn từ trước)
            await RunCheckAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                // Tính thời gian đến 00:00:01 ngày mai
                var now = DateTime.Now;
                var nextRun = DateTime.Today.AddDays(1).AddSeconds(1);
                var delay = nextRun - now;

                Log.Information("⏰ Lần chạy kế tiếp: {NextRun} (còn {Hours}h {Minutes}m)",
                    nextRun.ToString("yyyy-MM-dd HH:mm:ss"),
                    (int)delay.TotalHours, delay.Minutes);

                await Task.Delay(delay, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                    await RunCheckAsync(stoppingToken);
            }
        }

        private async Task RunCheckAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var now = DateTime.Now;
                var logs = new List<NhatKyHeThong>();

                // 1. Cửa hàng HoatDong/DungThu đã hết hạn → ChiDoc (grace period 15 ngày)
                var hetHan = await context.CuaHangs
                    .Where(c => (c.TrangThai == "HoatDong" || c.TrangThai == "DungThu")
                                && c.NgayHetHan < now)
                    .ToListAsync(stoppingToken);

                foreach (var store in hetHan)
                {
                    store.TrangThai = "ChiDoc";
                    Log.Warning("📖 [{Id}] {Ten} → ChiDoc (hết hạn {NgayHetHan:dd/MM/yyyy})",
                        store.Id, store.TenCuaHang, store.NgayHetHan);

                    logs.Add(new NhatKyHeThong
                    {
                        HanhDong = "TuDongKhoa",
                        MoTa = $"Cửa hàng [{store.Id}] {store.TenCuaHang} chuyển sang CHỈ ĐỌC (hết hạn {store.NgayHetHan:dd/MM/yyyy})",
                        UrlLienQuan = $"/super-admin/stores",
                        NguoiThucHien = "SYSTEM",
                        ThoiGian = now,
                        ChiTietJson = System.Text.Json.JsonSerializer.Serialize(new { storeId = store.Id, oldStatus = "HoatDong/DungThu", newStatus = "ChiDoc" })
                    });
                }

                // 2. ChiDoc quá 15 ngày → BiKhoa
                var quaHan = await context.CuaHangs
                    .Where(c => c.TrangThai == "ChiDoc" && c.NgayHetHan.AddDays(15) < now)
                    .ToListAsync(stoppingToken);

                foreach (var store in quaHan)
                {
                    store.TrangThai = "BiKhoa";
                    Log.Warning("🔒 [{Id}] {Ten} → BiKhoa (quá 15 ngày không gia hạn)", store.Id, store.TenCuaHang);

                    logs.Add(new NhatKyHeThong
                    {
                        HanhDong = "TuDongKhoa",
                        MoTa = $"Cửa hàng [{store.Id}] {store.TenCuaHang} BỊ KHÓA (quá 15 ngày không gia hạn)",
                        UrlLienQuan = $"/super-admin/stores",
                        NguoiThucHien = "SYSTEM",
                        ThoiGian = now,
                        ChiTietJson = System.Text.Json.JsonSerializer.Serialize(new { storeId = store.Id, oldStatus = "ChiDoc", newStatus = "BiKhoa" })
                    });
                }

                if (logs.Count > 0)
                {
                    context.NhatKyHeThangs.AddRange(logs);
                    await context.SaveChangesAsync(stoppingToken);
                }

                Log.Information("✅ Kiểm tra subscription xong: {ChiDoc} → ChiDoc, {BiKhoa} → BiKhoa",
                    hetHan.Count, quaHan.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Lỗi SubscriptionBackgroundService.RunCheckAsync");
            }
        }
    }
}
