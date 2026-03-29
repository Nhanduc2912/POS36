using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using System.Text;
using System.Text.Json;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // QUAN TRỌNG: Bắt buộc phải đăng nhập mới được chat với AI
    public class AIChatController : ControllerBase
    {
        private readonly string _geminiApiKey = "AIzaSyC1pMH7UEobMrIOdz2e9T9U53k1Frmh8bs";
        private readonly AppDbContext _context;

        public AIChatController(AppDbContext context)
        {
            _context = context;
        }

        // Hàm lấy ID cửa hàng hiện tại để bảo mật dữ liệu
        private int GetCuaHangId()
        {
            var claim = User.FindFirst("CuaHangId");
            if (claim == null) throw new UnauthorizedAccessException("Token không hợp lệ");
            return int.Parse(claim.Value);
        }

        // ==========================================
        // 1. API HỎI ĐÁP COPILOT (BƠM DỮ LIỆU TOÀN CỤC CHO QUẢN LÝ)
        // ==========================================
        [HttpPost("ask")]
        public async Task<IActionResult> AskCopilot([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(request.Question))
                return BadRequest("Câu hỏi không được để trống.");

            int cuaHangId = GetCuaHangId();
            string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_geminiApiKey}";

            // Lựa chọn file Prompt theo chức vụ (Cú pháp switch an toàn cho mọi bản C#)
            string roleFileName = "Chat_ThuNgan.md"; // Mặc định

            if (request.Role == "Order") roleFileName = "Chat_Order.md";
            else if (request.Role == "Admin" || request.Role == "QuanLy" || request.Role == "ChuCuaHang") roleFileName = "Chat_QuanLy.md";

            string promptPath = Path.Combine(Directory.GetCurrentDirectory(), "Prompts", roleFileName);
            if (!System.IO.File.Exists(promptPath)) return BadRequest($"Không tìm thấy file prompt: {roleFileName}");

            string systemPrompt = await System.IO.File.ReadAllTextAsync(promptPath);
            string liveDataSnippet = "";

            // NẾU LÀ SẾP LỚN -> BƠM DỮ LIỆU TỔNG QUAN TOÀN THỜI GIAN
            if (request.Role == "Admin" || request.Role == "QuanLy" || request.Role == "ChuCuaHang")
            {
                var now = DateTime.Now;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);

                // Dùng Entity Framework tính toán trực tiếp dưới SQL Server cho nhẹ RAM
                var queryHoaDon = _context.HoaDons.Where(h => h.CuaHangId == cuaHangId && h.TrangThai == "Đã thanh toán");

                decimal doanhThuHomNay = await queryHoaDon.Where(h => h.NgayTao >= now.Date).SumAsync(h => h.TongTien);
                decimal doanhThuThangNay = await queryHoaDon.Where(h => h.NgayTao >= startOfMonth).SumAsync(h => h.TongTien);
                decimal tongDoanhThuLichSu = await queryHoaDon.SumAsync(h => h.TongTien);
                int tongDonLichSu = await queryHoaDon.CountAsync();

                liveDataSnippet = $"\n[DỮ LIỆU BÍ MẬT CỦA QUÁN: Doanh thu hôm nay: {doanhThuHomNay:N0} VNĐ. Tháng này: {doanhThuThangNay:N0} VNĐ. Tổng doanh thu từ trước đến nay: {tongDoanhThuLichSu:N0} VNĐ với {tongDonLichSu} đơn. Chỉ sử dụng dữ liệu này nếu người dùng hỏi.]";
            }

            string fullPrompt = $"{systemPrompt}{liveDataSnippet}\n\nCâu hỏi của Sếp: {request.Question}";

            var payload = new { contents = new[] { new { parts = new[] { new { text = fullPrompt } } } } };
            using var client = new HttpClient();
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(endpoint, content);
                if (!response.IsSuccessStatusCode) return StatusCode(500, "Lỗi API AI");

                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonResponse);
                var aiText = doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();

                return Ok(new { answer = aiText });
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        // ==========================================
        // 2. API LẤY DANH SÁCH MODEL 
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
            catch (Exception ex) { return StatusCode(500, new { message = "Lỗi", error = ex.Message }); }
        }

        // ========================================================
        // 3. API BÁO CÁO AI (LẤY DỮ LIỆU TOÀN CẢNH)
        // ========================================================
        [HttpPost("report")]
        public async Task<IActionResult> GenerateReport([FromBody] AiReportRequest request)
        {
            try
            {
                int cuaHangId = GetCuaHangId();
                var now = DateTime.Now;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);

                var queryHoaDon = _context.HoaDons.Where(h => h.CuaHangId == cuaHangId && h.TrangThai == "Đã thanh toán");

                // 1. KÉO DỮ LIỆU THỐNG KÊ TOÀN CẢNH TỪ SQL SERVER
                decimal tongDoanhThuLichSu = await queryHoaDon.SumAsync(h => h.TongTien);
                decimal doanhThuThangNay = await queryHoaDon.Where(h => h.NgayTao >= startOfMonth).SumAsync(h => h.TongTien);
                decimal doanhThuHomNay = await queryHoaDon.Where(h => h.NgayTao >= now.Date).SumAsync(h => h.TongTien);
                int tongSoDonLichSu = await queryHoaDon.CountAsync();

                // Phân loại doanh thu hôm nay
                var hoaDonsHomNay = await queryHoaDon.Where(h => h.NgayTao >= now.Date).ToListAsync();
                decimal tienMatHomNay = hoaDonsHomNay.Where(h => h.PhuongThucThanhToan == "Tiền mặt").Sum(h => h.TongTien);
                decimal chuyenKhoanHomNay = hoaDonsHomNay.Where(h => h.PhuongThucThanhToan == "Chuyển khoản").Sum(h => h.TongTien);

                // Lấy Top 5 món "Gánh Team" từ trước đến nay
                var top5MonAllTime = await _context.ChiTietHoaDons
                    .Where(ct => ct.HoaDon!.CuaHangId == cuaHangId && ct.HoaDon.TrangThai == "Đã thanh toán")
                    .GroupBy(ct => ct.SanPham!.TenSanPham ?? "Khác")
                    .Select(g => new { Ten = g.Key, SL = g.Sum(x => x.SoLuong) })
                    .OrderByDescending(x => x.SL)
                    .Take(5)
                    .ToListAsync();

                string topMonString = top5MonAllTime.Any() ? string.Join(", ", top5MonAllTime.Select(x => $"{x.Ten} ({x.SL})")) : "Chưa có dữ liệu";

                // 2. ÉP THÀNH CHUỖI DỮ LIỆU "KHỦNG" CHO AI
                string rawData = $@"
DỮ LIỆU LỊCH SỬ KINH DOANH TOÀN DIỆN TÍNH ĐẾN {now:dd/MM/yyyy}:
- Tổng doanh thu lịch sử (All-time): {tongDoanhThuLichSu:N0} VND
- Số lượng hóa đơn lịch sử: {tongSoDonLichSu} đơn
- Doanh thu tháng hiện tại: {doanhThuThangNay:N0} VND
- Doanh thu riêng hôm nay: {doanhThuHomNay:N0} VND (Tiền mặt: {tienMatHomNay:N0} VND, Chuyển khoản: {chuyenKhoanHomNay:N0} VND)
- TOP 5 MÓN BÁN CHẠY NHẤT LỊCH SỬ: {topMonString}
";

                string systemPrompt = "";
                string promptPath = Path.Combine(Directory.GetCurrentDirectory(), "Prompts", "ReportCopilot.md");
                if (System.IO.File.Exists(promptPath))
                {
                    systemPrompt = await System.IO.File.ReadAllTextAsync(promptPath);
                }
                else
                {
                    systemPrompt = "Bạn là Chuyên gia dữ liệu. Trình bày báo cáo bằng bảng HTML. Phân tích số liệu và đưa ra chiến lược kinh doanh.";
                }

                string fullPrompt = $"{systemPrompt}\n\n{rawData}\n\nYêu cầu của người dùng: {request.Prompt}";
                string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_geminiApiKey}";

                var payload = new { contents = new[] { new { parts = new[] { new { text = fullPrompt } } } } };
                using var client = new HttpClient();
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(endpoint, content);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode) return StatusCode(500, jsonResponse);

                using var doc = JsonDocument.Parse(jsonResponse);
                var aiText = doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();

                aiText = aiText!.Replace("```html", "").Replace("```", "").Trim();

                return Ok(new { htmlReport = aiText });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi tạo báo cáo: " + ex.Message);
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