using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIChatController : ControllerBase
    {
        // THAY MÃ API KEY CỦA EM VÀO ĐÂY (Bắt đầu bằng AIza...)
        private readonly string _geminiApiKey = "AIzaSyC1pMH7UEobMrIOdz2e9T9U53k1Frmh8bs";

        [HttpPost("ask")]
        public async Task<IActionResult> AskCopilot([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(request.Question))
                return BadRequest("Câu hỏi không được để trống.");

            // 1. CÚ PHÁP GỌI MODEL GEMINI 1.5 FLASH CỦA GOOGLE
            // Sử dụng Gemini 2.5 Flash (Lấy từ đúng danh sách của em)
            string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_geminiApiKey}";

            // 2. MỚM LỜI HỆ THỐNG (SYSTEM PROMPT) - Bí quyết để AI trả lời đúng nghiệp vụ POS36
            string systemPrompt = @"
Bạn là 'POS36 Copilot', trợ lý AI thông minh của phần mềm quản lý nhà hàng POS36. 
Nhiệm vụ của bạn:
1. Trả lời thân thiện, ngắn gọn, xưng 'Mình' và gọi người dùng là 'Sếp' hoặc 'Bạn'.
2. Hướng dẫn sử dụng:
   - Nếu hỏi tách bàn: 'Bạn click đúp vào bàn, chọn nút Tách Bàn ở menu.'
   - Nếu hỏi in lại bill: 'Bạn vào Lịch sử hóa đơn, chọn đơn cần in và nhấn F3 nhé.'
   - Nếu hỏi chuyển bàn: 'Bạn chọn bàn hiện tại, bấm nút Chuyển bàn và trỏ sang bàn mới.'
3. Phân tích báo cáo: Nếu người dùng hỏi tình hình hôm nay, hãy tự bịa ra 1 báo cáo ảo siêu tích cực (vì hệ thống đang trong giai đoạn test) và động viên họ.
";

            // Trộn hướng dẫn hệ thống và câu hỏi của người dùng lại với nhau
            string fullPrompt = $"{systemPrompt}\n\nCâu hỏi của thu ngân: {request.Question}";

            // 3. ĐÓNG GÓI JSON GỬI LÊN GOOGLE
            var payload = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = fullPrompt } } }
                }
            };

            using var client = new HttpClient();
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                // BẮN LÊN GOOGLE VÀ CHỜ KẾT QUẢ
                var response = await client.PostAsync(endpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode(500, new { message = "Lỗi kết nối với Google AI", detail = error });
                }

                // 4. BÓC TÁCH CÂU TRẢ LỜI TỪ GOOGLE
                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonResponse);

                var aiText = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text").GetString();

                // Trả về cho Frontend Vue hiển thị
                return Ok(new { answer = aiText });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi gọi AI", error = ex.Message });
            }
        }
        // ==========================================
        // API LẤY DANH SÁCH TẤT CẢ MODEL CỦA GOOGLE
        // ==========================================
        [HttpGet("models")]
        public async Task<IActionResult> ListModels()
        {
            string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models?key={_geminiApiKey}";

            using var client = new HttpClient();
            try
            {
                var response = await client.GetAsync(endpoint);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                // FIX LỖI: Trả thẳng chuỗi JSON dưới dạng chữ nguyên thủy ra trình duyệt
                return Content(jsonResponse, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi gọi API", error = ex.Message });
            }
        }
    }

    public class ChatRequest
    {
        public string Question { get; set; } = string.Empty;
    }
}