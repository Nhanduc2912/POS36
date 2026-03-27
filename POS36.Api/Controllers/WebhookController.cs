using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using POS36.Api.Hubs;
using System.Text.RegularExpressions;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly IHubContext<KitchenHub> _hubContext; // Hoặc PosHub

        public WebhookController(IHubContext<KitchenHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost("sepay")]
        public async Task<IActionResult> SePayWebhook([FromBody] SePayPayload payload)
        {
            // 1. SỬA LẠI ĐIỀU KIỆN: Kiểm tra transferType là "in" (tiền vào) và transferAmount > 0
            if (payload == null || payload.transferType != "in" || payload.transferAmount <= 0 || string.IsNullOrEmpty(payload.content))
            {
                return Ok(new { success = false, message = "Bỏ qua (Không phải giao dịch tiền vào hợp lệ)" });
            }

            // 2. Tìm mã: "POS36B" cộng với số ID của bàn
            var match = Regex.Match(payload.content.ToUpper(), @"POS36B(\d+)");

            if (match.Success)
            {
                int banId = int.Parse(match.Groups[1].Value);

                // 3. Bắn SignalR chốt đơn!
                await _hubContext.Clients.All.SendAsync("ThanhToanQRThanhCong", banId);

                return Ok(new { success = true, message = $"Đã nhận {payload.transferAmount} cho Bàn {banId}" });
            }

            return Ok(new { success = false, message = "Giao dịch không chứa mã POS36 hợp lệ." });
        }
    }

    // CLASS MỚI: Khớp 100% với JSON của SePay bắn về
    public class SePayPayload
    {
        public decimal transferAmount { get; set; }  // Số tiền
        public string content { get; set; } = string.Empty; // Nội dung khách ghi
        public string transferType { get; set; } = string.Empty; // "in" hoặc "out"
        public string gateway { get; set; } = string.Empty; // Tên ngân hàng
    }
}