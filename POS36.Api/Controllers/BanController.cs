using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using POS36.Api.Models;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BanController : ControllerBase
    {
        private readonly AppDbContext _context;
        public BanController(AppDbContext context) { _context = context; }

        [HttpGet("khu-vuc/{khuVucId}")]
        public async Task<IActionResult> GetByKhuVuc(int khuVucId)
        {
            var ds = await _context.Bans.Where(b => b.KhuVucId == khuVucId).ToListAsync();
            return Ok(ds);
        }

        public class CreateBanDto { public int KhuVucId { get; set; } public string TenBan { get; set; } = string.Empty; public string TrangThai { get; set; } = string.Empty; }

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

        // Thêm API này vào BanController
        [HttpPost("tao-nhanh")]
        public async Task<IActionResult> TaoBanNhanh([FromBody] TaoBanNhanhDto request)
        {
            int cuaHangId = int.Parse(User.FindFirst("CuaHangId")?.Value ?? "0");

            // Đếm xem khu vực này đang có bao nhiêu bàn rồi
            int soBanHienTai = await _context.Bans.CountAsync(b => b.KhuVucId == request.KhuVucId && b.CuaHangId == cuaHangId);

            for (int i = 1; i <= request.SoLuong; i++)
            {
                _context.Bans.Add(new Ban
                {
                    CuaHangId = cuaHangId,
                    KhuVucId = request.KhuVucId,
                    TenBan = $"Bàn {soBanHienTai + i}", // Tự động tăng số
                    TrangThai = "Trống"
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Đã tạo thành công {request.SoLuong} bàn!" });
        }
    }
}