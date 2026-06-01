using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using POS36.Api.Data;
using POS36.Api.Hubs;
using Serilog;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly IHubContext<KitchenHub> _hubContext;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public WebhookController(IHubContext<KitchenHub> hubContext, AppDbContext context, IConfiguration configuration)
        {
            _hubContext = hubContext;
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("sepay")]
        public async Task<IActionResult> SePayWebhook([FromBody] SePayPayload payload)
        {
            // BUG-05 FIX: Xác thực API Key từ header Authorization của SePay
            var authHeader = Request.Headers["Authorization"].ToString();
            var configuredKey = _configuration["SePay:ApiKey"] ?? "sepay_webhook_secret_key_2026_xyz";

            if (string.IsNullOrEmpty(authHeader) || !authHeader.Equals($"Apikey {configuredKey}", StringComparison.OrdinalIgnoreCase))
            {
                Log.Warning("⚠️ Webhook SePay: Không có quyền truy cập hoặc sai API Key. Header: {Header}", authHeader);
                return Unauthorized(new { success = false, message = "Không có quyền truy cập!" });
            }

            // 1. Kiểm tra giao dịch hợp lệ
            if (payload == null || payload.transferType != "in" || payload.transferAmount <= 0 || string.IsNullOrEmpty(payload.content))
            {
                return Ok(new { success = false, message = "Bỏ qua (Không phải giao dịch tiền vào hợp lệ)" });
            }

            var content = payload.content.ToUpper();

            // ========== PATTERN 1: Thanh toán hóa đơn bàn ăn (POS36B{banId}) ==========
            var matchBill = Regex.Match(content, @"POS36B(\d+)");
            if (matchBill.Success)
            {
                int banId = int.Parse(matchBill.Groups[1].Value);

                // Tìm cửa hàng sở hữu bàn này để gửi đúng group
                var ban = await _context.Bans.FirstOrDefaultAsync(b => b.Id == banId);
                var cuaHangId = ban?.CuaHangId ?? 0;

                await _hubContext.Clients.Group($"store_{cuaHangId}").SendAsync("ThanhToanQRThanhCong", banId);
                Log.Information("💰 SePay: Nhận {Amount} cho Bàn {BanId} (CuaHang {CuaHangId})", payload.transferAmount, banId, cuaHangId);
                return Ok(new { success = true, message = $"Đã nhận {payload.transferAmount} cho Bàn {banId}" });
            }

            // ========== PATTERN 2: Thanh toán gói dịch vụ SaaS (POS36G{orderId}) ==========
            var matchSub = Regex.Match(content, @"POS36G(\d+)");
            if (matchSub.Success)
            {
                int orderId = int.Parse(matchSub.Groups[1].Value);

                var lichSu = await _context.LichSuDangKys
                    .Include(l => l.GoiDichVu)
                    .FirstOrDefaultAsync(l => l.Id == orderId && l.TrangThai == "ChoThanhToan");

                if (lichSu == null)
                {
                    Log.Warning("⚠️ SePay: Đơn POS36G{OrderId} không tồn tại hoặc đã thanh toán.", orderId);
                    return Ok(new { success = false, message = "Đơn không tồn tại hoặc đã xử lý." });
                }

                // Kiểm tra số tiền khớp (cho phép sai lệch ±1000đ vì ngân hàng có thể làm tròn)
                if (Math.Abs(payload.transferAmount - lichSu.SoTienThanhToan) > 1000)
                {
                    Log.Warning("⚠️ SePay: Số tiền không khớp cho POS36G{OrderId}. Nhận: {Received}, Cần: {Expected}",
                        orderId, payload.transferAmount, lichSu.SoTienThanhToan);
                    return Ok(new { success = false, message = "Số tiền chuyển khoản không khớp." });
                }

                // Duyệt tự động
                lichSu.TrangThai = "DaThanhToan";
                lichSu.NgayThanhToan = DateTime.Now;
                lichSu.NguoiDuyet = "SePay-Auto";
                lichSu.PhuongThucThanhToan = "SePay";

                // Gia hạn cửa hàng
                var store = await _context.CuaHangs.FindAsync(lichSu.CuaHangId);
                if (store != null && lichSu.GoiDichVu != null)
                {
                    var ngayBatDau = (store.NgayHetHan > DateTime.Now) ? store.NgayHetHan : DateTime.Now;
                    store.NgayHetHan = ngayBatDau.AddMonths(lichSu.GoiDichVu.SoThang);
                    store.TrangThai = "HoatDong";
                    store.GoiDichVu = lichSu.GoiDichVu.MaGoi;
                }

                await _context.SaveChangesAsync();

                Log.Information("🎉 SePay AUTO: Duyệt đơn POS36G{OrderId} — {Amount}đ cho cửa hàng [{CuaHangId}]",
                    orderId, payload.transferAmount, lichSu.CuaHangId);

                return Ok(new { success = true, message = $"Đã kích hoạt gói cho đơn POS36G{orderId}" });
            }

            return Ok(new { success = false, message = "Giao dịch không chứa mã POS36 hợp lệ." });
        }
    }

    // CLASS: Khớp 100% với JSON của SePay bắn về
    public class SePayPayload
    {
        public decimal transferAmount { get; set; }
        public string content { get; set; } = string.Empty;
        public string transferType { get; set; } = string.Empty;
        public string gateway { get; set; } = string.Empty;
    }
}