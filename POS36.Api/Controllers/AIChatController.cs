using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using POS36.Api.Data; // Cần cái này để gọi AppDbContext
using Microsoft.EntityFrameworkCore; // Cần cái này để dùng ToListAsync, Where...

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIChatController : ControllerBase
    {
        // MÃ API KEY CỦA SẾP
        private readonly string _geminiApiKey = "AIzaSyC1pMH7UEobMrIOdz2e9T9U53k1Frmh8bs";

        // BIẾN KẾT NỐI DATABASE
        private readonly AppDbContext _context;

        // INJECT DATABASE VÀO CONTROLLER (Chính là chỗ sếp bị thiếu)
        public AIChatController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. API HỎI ĐÁP COPILOT
        // ==========================================
        [HttpPost("ask")]
        public async Task<IActionResult> AskCopilot([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(request.Question))
                return BadRequest("Câu hỏi không được để trống.");

            string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_geminiApiKey}";

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

            string fullPrompt = $"{systemPrompt}\n\nCâu hỏi của thu ngân: {request.Question}";

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
                var response = await client.PostAsync(endpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode(500, new { message = "Lỗi kết nối với Google AI", detail = error });
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonResponse);

                var aiText = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text").GetString();

                return Ok(new { answer = aiText });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi gọi AI", error = ex.Message });
            }
        }

        // ==========================================
        // 2. API LẤY DANH SÁCH TẤT CẢ MODEL CỦA GOOGLE
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

                return Content(jsonResponse, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi gọi API", error = ex.Message });
            }
        }

        // ========================================================
        // 3. API MỚI: TẠO BÁO CÁO AI BẰNG PROMPT THÔNG MINH
        // ========================================================
        [HttpPost("report")]
        public async Task<IActionResult> GenerateReport([FromBody] AiReportRequest request)
        {
            try
            {
                // 1. LẤY DỮ LIỆU THẬT TỪ DATABASE
                var today = DateTime.Now.Date;
                var hoaDons = await _context.HoaDons
                    .Include(h => h.ChiTietHoaDons!)
                    .ThenInclude(ct => ct.SanPham)
                    .Where(h => h.NgayTao >= today && h.TrangThai == "Đã thanh toán")
                    .ToListAsync();

                decimal tongDoanhThu = hoaDons.Sum(h => h.TongTien);
                int tongDon = hoaDons.Count;
                int tongSoLy = hoaDons.SelectMany(h => h.ChiTietHoaDons!).Sum(ct => ct.SoLuong);

                // Gom nhóm tìm món bán chạy
                var topMon = hoaDons.SelectMany(h => h.ChiTietHoaDons!)
                    .GroupBy(ct => ct.SanPham?.TenSanPham ?? "SP Khác")
                    .Select(g => new { Ten = g.Key, SL = g.Sum(x => x.SoLuong) })
                    .OrderByDescending(x => x.SL)
                    .FirstOrDefault();

                // 2. GÓI DỮ LIỆU THÔ THÀNH CHUỖI NHỎ
                string rawData = $@"
Dữ liệu ngày {today:dd/MM/yyyy}:
- Tổng doanh thu: {tongDoanhThu:N0} VND
- Tổng số đơn: {tongDon}
- Tổng SP bán ra: {tongSoLy}
- Món bán chạy nhất: {topMon?.Ten ?? "Chưa có"} (SL: {topMon?.SL ?? 0})
";

                // 3. MỚM LỜI CHO AI (SYSTEM PROMPT)
                string systemPrompt = @"
Bạn là Chuyên gia phân tích dữ liệu POS36. Dựa vào dữ liệu thô cung cấp, hãy trả lời đúng yêu cầu của người dùng.
QUAN TRỌNG: 
- Nếu người dùng yêu cầu 'báo cáo', 'bảng', 'thống kê', HÃY TRÌNH BÀY DƯỚI DẠNG BẢNG HTML CƠ BẢN (dùng thẻ <table>, <tr>, <th>, <td>). Không dùng Markdown table.
- Thêm các nhận xét phân tích ở dưới bảng.
";

                string fullPrompt = $"{systemPrompt}\n\n{rawData}\n\nYêu cầu của người dùng: {request.Prompt}";

                // 4. GỌI GEMINI API
                string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_geminiApiKey}";

                var payload = new { contents = new[] { new { parts = new[] { new { text = fullPrompt } } } } };
                using var client = new HttpClient();
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(endpoint, content);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode) return StatusCode(500, jsonResponse);

                using var doc = JsonDocument.Parse(jsonResponse);
                var aiText = doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();

                // Dọn dẹp thẻ code html rác
                aiText = aiText!.Replace("```html", "").Replace("```", "").Trim();

                return Ok(new { htmlReport = aiText });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi tạo báo cáo: " + ex.Message);
            }
        }
    }

    // CÁC CLASS DTO HỨNG DỮ LIỆU
    public class AiReportRequest
    {
        public string Prompt { get; set; } = string.Empty;
    }

    public class ChatRequest
    {
        public string Question { get; set; } = string.Empty;
    }
}