using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using System.Text;
using System.Text.Json;
using Serilog; // Bắt buộc phải có để in màu ra Terminal

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Bắt buộc đăng nhập
    public class AIChatController : ControllerBase
    {
        private readonly string _geminiApiKey;
        private readonly AppDbContext _context;

        public AIChatController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _geminiApiKey = configuration["GeminiAI:ApiKey"] ?? "";
        }

        // ==========================================
        // HÀM LẤY ID AN TOÀN (CHỐNG CRASH LỖI 500)
        // ==========================================
        private int GetCuaHangId()
        {
            var claim = User.FindFirst("CuaHangId");
            if (claim == null || string.IsNullOrEmpty(claim.Value))
            {
                Log.Warning("⚠️ Token không có CuaHangId. Tạm gán CuaHangId = 1 để test AI.");
                return 1; // Chống crash hệ thống nếu Admin chưa có Cửa hàng
            }
            return int.TryParse(claim.Value, out int id) ? id : 1;
        }

        // ==========================================
        // 1. API HỎI ĐÁP COPILOT (ĐÃ UPDATE 2.5 FLASH)
        // ==========================================
        [HttpPost("ask")]
        public async Task<IActionResult> AskCopilot([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(request.Question)) return BadRequest("Câu hỏi trống.");

            int cuaHangId = GetCuaHangId();

            // CHUẨN HÓA SANG BẢN 2.5 FLASH
            string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_geminiApiKey}";

            string roleFileName = request.Role switch
            {
                "Order" => "Chat_Order.md",
                "Admin" or "QuanLy" or "ChuCuaHang" => "Chat_QuanLy.md",
                _ => "Chat_ThuNgan.md"
            };

            string promptPath = Path.Combine(Directory.GetCurrentDirectory(), "Prompts", roleFileName);
            if (!System.IO.File.Exists(promptPath)) return BadRequest($"Không tìm thấy file: {roleFileName}");

            string systemPrompt = await System.IO.File.ReadAllTextAsync(promptPath);
            string liveDataSnippet = "";

            if (request.Role == "Admin" || request.Role == "QuanLy" || request.Role == "ChuCuaHang")
            {
                var now = DateTime.Now;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var queryHoaDon = _context.HoaDons.Where(h => h.CuaHangId == cuaHangId && h.TrangThai == "Đã thanh toán");

                decimal dtHomNay = await queryHoaDon.Where(h => h.NgayTao >= now.Date).SumAsync(h => h.TongTien);
                decimal dtThangNay = await queryHoaDon.Where(h => h.NgayTao >= startOfMonth).SumAsync(h => h.TongTien);
                decimal dtLichSu = await queryHoaDon.SumAsync(h => h.TongTien);
                int tongDon = await queryHoaDon.CountAsync();

                liveDataSnippet = $"\n[DỮ LIỆU: Hôm nay {dtHomNay:N0} VNĐ. Tháng này {dtThangNay:N0} VNĐ. Tổng {dtLichSu:N0} VNĐ ({tongDon} đơn).]";
            }

            string fullPrompt = $"{systemPrompt}{liveDataSnippet}\n\nCâu hỏi Sếp: {request.Question}";
            var payload = new { contents = new[] { new { parts = new[] { new { text = fullPrompt } } } } };

            using var client = new HttpClient();
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(endpoint, content);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // IN ĐÍCH DANH CÂU CHỬI CỦA GOOGLE RA TERMINAL
                    Log.Error("❌ Lỗi API từ Google Gemini (AskCopilot): {Error}", jsonResponse);
                    return StatusCode(500, $"Lỗi từ Google: {jsonResponse}");
                }

                using var doc = JsonDocument.Parse(jsonResponse);
                var aiText = doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();

                return Ok(new { answer = aiText });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Code C# bị sập tại hàm AskCopilot!");
                return StatusCode(500, ex.Message);
            }
        }

        // ==========================================
        // 2. API LẤY DANH SÁCH MODEL (ĐÃ SỬA CÚ PHÁP CHUẨN)
        // ==========================================
        [HttpGet("models")]
        public async Task<IActionResult> ListModels()
        {
            // Cú pháp lấy list model CHUẨN (không có chữ generateContent)
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
                Log.Error(ex, "❌ Lỗi khi lấy danh sách Models");
                return StatusCode(500, new { message = "Lỗi", error = ex.Message });
            }
        }

        // ========================================================
        // 3. API BÁO CÁO AI (ĐÃ UPDATE 2.5 FLASH)
        // ========================================================
        [HttpPost("report")]
        public async Task<IActionResult> GenerateReport([FromBody] AiReportRequest request)
        {
            try
            {
                int cuaHangId = GetCuaHangId();
                var now = DateTime.Now;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
                var startOfWeek = now.Date.AddDays(-1 * diff);

                var queryHoaDon = _context.HoaDons.Where(h => h.CuaHangId == cuaHangId && h.TrangThai == "Đã thanh toán");

                // --- TÍNH TOÁN DỮ LIỆU ---
                decimal tongDoanhThu = await queryHoaDon.SumAsync(h => h.TongTien);
                int tongDon = await queryHoaDon.CountAsync();

                var hdThang = await queryHoaDon.Where(h => h.NgayTao >= startOfMonth).ToListAsync();
                decimal dtThang = hdThang.Sum(h => h.TongTien);
                decimal ckThang = hdThang.Where(h => h.PhuongThucThanhToan == "Chuyển khoản").Sum(h => h.TongTien);
                decimal tmThang = hdThang.Where(h => h.PhuongThucThanhToan == "Tiền mặt").Sum(h => h.TongTien);

                var hdTuan = hdThang.Where(h => h.NgayTao >= startOfWeek).ToList();
                decimal dtTuan = hdTuan.Sum(h => h.TongTien);
                decimal ckTuan = hdTuan.Where(h => h.PhuongThucThanhToan == "Chuyển khoản").Sum(h => h.TongTien);
                decimal tmTuan = hdTuan.Where(h => h.PhuongThucThanhToan == "Tiền mặt").Sum(h => h.TongTien);

                var hdHomNay = hdTuan.Where(h => h.NgayTao >= now.Date).ToList();
                decimal dtHomNay = hdHomNay.Sum(h => h.TongTien);
                decimal ckHomNay = hdHomNay.Where(h => h.PhuongThucThanhToan == "Chuyển khoản").Sum(h => h.TongTien);
                decimal tmHomNay = hdHomNay.Where(h => h.PhuongThucThanhToan == "Tiền mặt").Sum(h => h.TongTien);

                var top5MonAllTime = await _context.ChiTietHoaDons
                    .Where(ct => ct.HoaDon!.CuaHangId == cuaHangId && ct.HoaDon.TrangThai == "Đã thanh toán")
                    .GroupBy(ct => ct.SanPham!.TenSanPham ?? "Khác")
                    .Select(g => new { Ten = g.Key, SL = g.Sum(x => x.SoLuong) })
                    .OrderByDescending(x => x.SL)
                    .Take(5)
                    .ToListAsync();
                string topMonString = top5MonAllTime.Any() ? string.Join(", ", top5MonAllTime.Select(x => $"{x.Ten} ({x.SL})")) : "Chưa có";

                string rawData = $@"
DỮ LIỆU KINH DOANH TÍNH ĐẾN {now:dd/MM/yyyy}:
[LỊCH SỬ] Doanh thu: {tongDoanhThu:N0} VND ({tongDon} đơn). TOP 5 MÓN: {topMonString}
[THÁNG NÀY] Doanh thu: {dtThang:N0} VND (Tiền mặt: {tmThang:N0}, Chuyển khoản: {ckThang:N0})
[TUẦN NÀY] Doanh thu: {dtTuan:N0} VND (Tiền mặt: {tmTuan:N0}, Chuyển khoản: {ckTuan:N0})
[HÔM NAY] Doanh thu: {dtHomNay:N0} VND (Tiền mặt: {tmHomNay:N0}, Chuyển khoản: {ckHomNay:N0})
";

                string promptPath = Path.Combine(Directory.GetCurrentDirectory(), "Prompts", "ReportCopilot.md");
                string systemPrompt = System.IO.File.Exists(promptPath) ? await System.IO.File.ReadAllTextAsync(promptPath) : "Trả lời dưới dạng HTML.";
                string fullPrompt = $"{systemPrompt}\n\n{rawData}\n\nYêu cầu người dùng: {request.Prompt}";

                // CHUẨN HÓA SANG BẢN 2.5 FLASH
                string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_geminiApiKey}";

                var payload = new { contents = new[] { new { parts = new[] { new { text = fullPrompt } } } } };
                using var client = new HttpClient();
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(endpoint, content);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Log.Error("❌ Lỗi API từ Google Gemini (Report): {Error}", jsonResponse);
                    return StatusCode(500, $"Lỗi Google: {jsonResponse}");
                }

                using var doc = JsonDocument.Parse(jsonResponse);
                var aiText = doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();

                aiText = aiText!.Replace("```html", "").Replace("```", "").Trim();
                return Ok(new { htmlReport = aiText });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Code C# bị sập tại hàm GenerateReport!");
                return StatusCode(500, ex.Message);
            }
        }
    }

    public class AiReportRequest
    {
        public string Prompt { get; set; } = string.Empty;
    }

    public class ChatRequest
    {
        public string Question { get; set; } = string.Empty;
        public string Role { get; set; } = "ThuNgan";
    }
}