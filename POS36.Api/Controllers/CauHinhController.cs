using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using POS36.Api.Models;
using Serilog;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class CauHinhController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContext;

        public CauHinhController(AppDbContext context, IHttpContextAccessor httpContext)
        {
            _context = context;
            _httpContext = httpContext;
        }

        // Helper: lấy IP
        private string? GetClientIp() =>
            _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        // Helper: ghi nhật ký
        private async Task GhiLog(string hanhDong, string moTa, string? url = null, string? chiTiet = null)
        {
            _context.NhatKyHeThangs.Add(new NhatKyHeThong
            {
                HanhDong = hanhDong,
                MoTa = moTa,
                UrlLienQuan = url,
                IpAddress = GetClientIp(),
                NguoiThucHien = User.Identity?.Name ?? User.FindFirst("TenDangNhap")?.Value,
                ThoiGian = DateTime.Now,
                ChiTietJson = chiTiet,
            });
            await _context.SaveChangesAsync();
        }

        // ==========================================
        // 1. LẤY TẤT CẢ CẤU HÌNH (nhóm theo nhóm)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? nhom)
        {
            var query = _context.CauHinhHeThangs.AsQueryable();
            if (!string.IsNullOrEmpty(nhom))
                query = query.Where(c => c.NhomCauHinh == nhom);

            var list = await query
                .OrderBy(c => c.NhomCauHinh)
                .ThenBy(c => c.MaKey)
                .Select(c => new
                {
                    c.Id, c.NhomCauHinh, c.MaKey, c.GiaTri, c.MoTa, c.NgayCapNhat, c.NguoiCapNhat
                })
                .ToListAsync();

            // Group theo nhóm
            var grouped = list.GroupBy(x => x.NhomCauHinh)
                .ToDictionary(g => g.Key, g => g.ToList());

            return Ok(grouped);
        }

        // ==========================================
        // 2. CẬP NHẬT NHIỀU KEYS CÙNG LÚC (batch)
        // ==========================================
        [HttpPut("batch")]
        public async Task<IActionResult> BatchUpdate([FromBody] List<CauHinhBatchDto> items)
        {
            if (items == null || !items.Any())
                return BadRequest("Không có dữ liệu để cập nhật!");

            var nguoi = User.Identity?.Name ?? User.FindFirst("TenDangNhap")?.Value;
            var changed = new List<string>();

            foreach (var item in items)
            {
                var existing = await _context.CauHinhHeThangs
                    .FirstOrDefaultAsync(c => c.NhomCauHinh == item.NhomCauHinh && c.MaKey == item.MaKey);

                if (existing != null)
                {
                    var oldValue = existing.GiaTri;
                    existing.GiaTri = item.GiaTri ?? "";
                    existing.NgayCapNhat = DateTime.Now;
                    existing.NguoiCapNhat = nguoi;
                    changed.Add($"{item.MaKey}: [{oldValue}] → [{item.GiaTri}]");
                }
                else
                {
                    // Tạo mới nếu key chưa tồn tại
                    _context.CauHinhHeThangs.Add(new CauHinhHeThong
                    {
                        NhomCauHinh = item.NhomCauHinh,
                        MaKey = item.MaKey,
                        GiaTri = item.GiaTri ?? "",
                        MoTa = item.MoTa,
                        NgayCapNhat = DateTime.Now,
                        NguoiCapNhat = nguoi,
                    });
                    changed.Add($"{item.MaKey}: [NEW] = [{item.GiaTri}]");
                }
            }

            await _context.SaveChangesAsync();

            // Ghi nhật ký
            await GhiLog("CauHinh", $"Cập nhật {changed.Count} cấu hình",
                "/super-admin/config",
                System.Text.Json.JsonSerializer.Serialize(changed));

            Log.Information("⚙️ SuperAdmin cập nhật cấu hình: {Changes}", string.Join(", ", changed));
            return Ok(new { message = $"Đã cập nhật {changed.Count} cấu hình!" });
        }

        // ==========================================
        // 3. LẤY NHẬT KÝ HỆ THỐNG (phân trang)
        // ==========================================
        [HttpGet("nhat-ky")]
        public async Task<IActionResult> GetNhatKy(
            [FromQuery] string? hanhDong,
            [FromQuery] string? tuNgay,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var query = _context.NhatKyHeThangs.AsQueryable();

            if (!string.IsNullOrEmpty(hanhDong))
                query = query.Where(n => n.HanhDong == hanhDong);

            if (DateTime.TryParse(tuNgay, out var fromDate))
                query = query.Where(n => n.ThoiGian >= fromDate);

            int total = await query.CountAsync();

            var logs = await query
                .OrderByDescending(n => n.ThoiGian)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new
                {
                    n.Id, n.HanhDong, n.MoTa, n.UrlLienQuan,
                    n.IpAddress, n.NguoiThucHien, n.ThoiGian
                })
                .ToListAsync();

            return Ok(new { total, page, pageSize, data = logs });
        }

        // ==========================================
        // 4. LẤY CHI TIẾT 1 LOG ENTRY
        // ==========================================
        [HttpGet("nhat-ky/{id}")]
        public async Task<IActionResult> GetNhatKyDetail(int id)
        {
            var log = await _context.NhatKyHeThangs.FindAsync(id);
            if (log == null) return NotFound("Không tìm thấy bản ghi!");
            return Ok(log);
        }

        // ==========================================
        // 5. TEST EMAIL (kiểm tra cấu hình EmailJS)
        // ==========================================
        [HttpPost("test-email")]
        public async Task<IActionResult> TestEmail([FromBody] TestEmailDto dto)
        {
            if (string.IsNullOrEmpty(dto.To))
                return BadRequest("Vui lòng nhập địa chỉ email nhận!");

            // Ghi log thao tác test
            await GhiLog("TestEmail", $"Test gửi email đến {dto.To}", "/super-admin/config");

            // Thực tế sẽ gọi SMTP hoặc trả về token cho frontend tự gửi qua EmailJS
            return Ok(new
            {
                message = "Yêu cầu test email đã được ghi nhận. Frontend sẽ gửi qua EmailJS với public key đã cấu hình.",
                emailJsReady = true,
            });
        }
    }

    // DTOs
    public class CauHinhBatchDto
    {
        public string NhomCauHinh { get; set; } = string.Empty;
        public string MaKey { get; set; } = string.Empty;
        public string? GiaTri { get; set; }
        public string? MoTa { get; set; }
    }

    public class TestEmailDto
    {
        public string To { get; set; } = string.Empty;
        public string? Subject { get; set; }
    }
}
