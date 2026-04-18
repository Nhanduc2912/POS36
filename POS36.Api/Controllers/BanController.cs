using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using POS36.Api.Models;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // BUG #1 FIX: Bảo vệ toàn bộ controller, bắt buộc đăng nhập
    public class BanController : ControllerBase
    {
        private readonly AppDbContext _context;
        public BanController(AppDbContext context) { _context = context; }

        private int GetCuaHangId()
        {
            var claim = User.FindFirst("CuaHangId");
            if (claim == null) throw new UnauthorizedAccessException("Token không hợp lệ");
            return int.Parse(claim.Value);
        }

        [HttpGet("khu-vuc/{khuVucId}")]
        public async Task<IActionResult> GetByKhuVuc(int khuVucId)
        {
            int cuaHangId = GetCuaHangId();
            var ds = await _context.Bans
                .Where(b => b.KhuVucId == khuVucId && b.CuaHangId == cuaHangId)
                .ToListAsync();
            return Ok(ds);
        }

        public class CreateBanDto { public int KhuVucId { get; set; } public string TenBan { get; set; } = string.Empty; public string TrangThai { get; set; } = string.Empty; }

        // BUG #12 FIX: Chỉ ChuCuaHang mới được tạo bàn mới
        [Authorize(Roles = "ChuCuaHang")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBanDto req)
        {
            var newTable = new Ban { KhuVucId = req.KhuVucId, TenBan = req.TenBan, TrangThai = req.TrangThai };
            _context.Bans.Add(newTable);
            await _context.SaveChangesAsync();
            return Ok(newTable);
        }
        [HttpGet("danh-sach-pos")]
        public async Task<IActionResult> GetDanhSachPos([FromQuery] int chiNhanhId)
        {
            var bans = await _context.Bans
                .Include(b => b.KhuVuc)
                .Where(b => b.KhuVuc!.ChiNhanhId == chiNhanhId)
                .Select(b => new
                {
                    b.Id,
                    b.TenBan,
                    b.TrangThai,
                    // Tìm cái Hóa Đơn Đang phục vụ của bàn này
                    HoaDon = _context.HoaDons
                        .Where(h => h.BanId == b.Id && h.TrangThai == "Đang phục vụ")
                        .OrderByDescending(h => h.Id)
                        .FirstOrDefault()
                })
                .Select(x => new
                {
                    x.Id,
                    x.TenBan,
                    x.TrangThai,
                    // Lấy Giờ và Tiền THẬT TẾ từ Hóa Đơn ra
                    TimeOpen = x.HoaDon != null ? x.HoaDon.NgayTao : (DateTime?)null,
                    TamTinh = x.HoaDon != null ? x.HoaDon.TongTien : 0
                })
                .ToListAsync();

            return Ok(bans);
        }
        // Nhớ thêm class DTO này ở đầu file hoặc file DTO riêng
        public class TaoBanNhanhDto
        {
            public int KhuVucId { get; set; }
            public int SoLuong { get; set; }
        }

        // BUG #12 FIX: Chỉ ChuCuaHang mới được tạo nhành nhiều bàn
        [Authorize(Roles = "ChuCuaHang")]
        [HttpPost("tao-nhanh")]
        public async Task<IActionResult> TaoBanNhanh([FromBody] TaoBanNhanhDto request)
        {
            int cuaHangId = GetCuaHangId();

            // BUG #11 FIX: Dùng số bàn lớn nhất hiện tại thay vì count
            // Tránh trùng tên khi đã xóa bàn giữa chừng (VD: xóa Bàn 3, count=3 nhưng Bàn 4 đã tồn tại)
            var tenBanList = await _context.Bans
                .Where(b => b.KhuVucId == request.KhuVucId && b.CuaHangId == cuaHangId)
                .Select(b => b.TenBan)
                .ToListAsync();

            int soLonNhat = tenBanList
                .Select(ten => {
                    var parts = ten.Split(' ');
                    return (parts.Length > 1 && int.TryParse(parts[^1], out int n)) ? n : 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            for (int i = 1; i <= request.SoLuong; i++)
            {
                _context.Bans.Add(new Ban
                {
                    CuaHangId = cuaHangId,
                    KhuVucId = request.KhuVucId,
                    TenBan = $"Bàn {soLonNhat + i}",
                    TrangThai = "Trống"
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Đã tạo thành công {request.SoLuong} bàn!" });
        }

    }
}