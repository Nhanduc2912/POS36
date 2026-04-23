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
        private const string TrangThaiAn = "Ẩn";
        private static bool IsTrangThaiAn(string? trangThai) =>
            string.Equals(trangThai?.Trim(), TrangThaiAn, StringComparison.OrdinalIgnoreCase);
        private static bool IsTrangThaiTrong(string? trangThai) =>
            string.IsNullOrWhiteSpace(trangThai) ||
            string.Equals(trangThai.Trim(), "Trống", StringComparison.OrdinalIgnoreCase);

        private int GetCuaHangId()
        {
            var claim = User.FindFirst("CuaHangId");
            if (claim == null) throw new UnauthorizedAccessException("Token không hợp lệ");
            return int.Parse(claim.Value);
        }

        [HttpGet("khu-vuc/{khuVucId}")]
        public async Task<IActionResult> GetByKhuVuc(int khuVucId, [FromQuery] bool includeHidden = false)
        {
            int cuaHangId = GetCuaHangId();
            var query = _context.Bans
                .Where(b => b.KhuVucId == khuVucId && b.CuaHangId == cuaHangId);

            if (!includeHidden)
                query = query.Where(b => b.TrangThai != TrangThaiAn);

            var ds = await query
                .ToListAsync();
            return Ok(ds);
        }

        public class CreateBanDto { public int KhuVucId { get; set; } public string TenBan { get; set; } = string.Empty; public string TrangThai { get; set; } = string.Empty; }
        public class UpdateBanDto { public string TenBan { get; set; } = string.Empty; }
        public class SetHienThiDto { public bool HienThi { get; set; } }

        // BUG #12 FIX: Chỉ ChuCuaHang mới được tạo bàn mới
        [Authorize(Roles = "ChuCuaHang,QuanLy,Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBanDto req)
        {
            int cuaHangId = GetCuaHangId();

            var khuVucHopLe = await _context.KhuVucs
                .AnyAsync(kv => kv.Id == req.KhuVucId && kv.CuaHangId == cuaHangId);
            if (!khuVucHopLe)
                return BadRequest("Khu vực không thuộc cửa hàng hiện tại.");

            var tenBan = req.TenBan.Trim();
            if (string.IsNullOrWhiteSpace(tenBan))
                return BadRequest("Tên bàn không được để trống.");

            bool isTrungTen = await _context.Bans.AnyAsync(b =>
                b.CuaHangId == cuaHangId &&
                b.KhuVucId == req.KhuVucId &&
                b.TenBan.ToLower() == tenBan.ToLower());
            if (isTrungTen)
                return BadRequest("Tên bàn đã tồn tại trong khu vực.");

            var newTable = new Ban
            {
                CuaHangId = cuaHangId,
                KhuVucId = req.KhuVucId,
                TenBan = tenBan,
                TrangThai = string.IsNullOrWhiteSpace(req.TrangThai) ? "Trống" : req.TrangThai
            };
            _context.Bans.Add(newTable);
            await _context.SaveChangesAsync();
            return Ok(newTable);
        }

        [Authorize(Roles = "ChuCuaHang,QuanLy,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBanDto req)
        {
            int cuaHangId = GetCuaHangId();
            var ban = await _context.Bans
                .FirstOrDefaultAsync(b => b.Id == id && b.CuaHangId == cuaHangId);
            if (ban == null)
                return NotFound("Không tìm thấy bàn.");

            if (IsTrangThaiAn(ban.TrangThai))
                return BadRequest("Bàn đã ẩn, không thể chỉnh sửa.");

            if (!IsTrangThaiTrong(ban.TrangThai))
                return BadRequest("Chỉ được sửa bàn khi bàn đang trống.");

            var tenBanMoi = req.TenBan.Trim();
            if (string.IsNullOrWhiteSpace(tenBanMoi))
                return BadRequest("Tên bàn không được để trống.");

            bool isTrungTen = await _context.Bans.AnyAsync(b =>
                b.Id != id &&
                b.CuaHangId == cuaHangId &&
                b.KhuVucId == ban.KhuVucId &&
                b.TenBan.ToLower() == tenBanMoi.ToLower());
            if (isTrungTen)
                return BadRequest("Tên bàn đã tồn tại trong khu vực.");

            ban.TenBan = tenBanMoi;
            await _context.SaveChangesAsync();
            return Ok(ban);
        }

        [Authorize(Roles = "ChuCuaHang,QuanLy,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            int cuaHangId = GetCuaHangId();
            var ban = await _context.Bans
                .FirstOrDefaultAsync(b => b.Id == id && b.CuaHangId == cuaHangId);
            if (ban == null)
                return NotFound("Không tìm thấy bàn.");

            if (IsTrangThaiAn(ban.TrangThai))
                return BadRequest("Bàn đã ở trạng thái ẩn.");

            if (!IsTrangThaiTrong(ban.TrangThai))
                return BadRequest("Chỉ được ẩn bàn khi bàn đang trống.");

            ban.TrangThai = TrangThaiAn;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã ẩn bàn thành công." });
        }

        [Authorize(Roles = "ChuCuaHang,QuanLy,Admin")]
        [HttpPut("{id}/hien-thi")]
        public async Task<IActionResult> SetHienThi(int id, [FromBody] SetHienThiDto req)
        {
            int cuaHangId = GetCuaHangId();
            var ban = await _context.Bans
                .FirstOrDefaultAsync(b => b.Id == id && b.CuaHangId == cuaHangId);
            if (ban == null)
                return NotFound("Không tìm thấy bàn.");

            if (!req.HienThi)
            {
                if (IsTrangThaiAn(ban.TrangThai))
                    return Ok(new { message = "Bàn đã ẩn sẵn." });
                if (!IsTrangThaiTrong(ban.TrangThai))
                    return BadRequest("Chỉ được ẩn bàn khi bàn đang trống.");

                ban.TrangThai = TrangThaiAn;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Đã ẩn bàn thành công." });
            }

            if (!IsTrangThaiAn(ban.TrangThai))
                return Ok(new { message = "Bàn đang hiển thị." });

            bool isTrungTen = await _context.Bans.AnyAsync(b =>
                b.Id != id &&
                b.CuaHangId == cuaHangId &&
                b.KhuVucId == ban.KhuVucId &&
                b.TrangThai != TrangThaiAn &&
                b.TenBan.ToLower() == ban.TenBan.ToLower());
            if (isTrungTen)
                return BadRequest("Không thể bật lại vì tên bàn đang bị trùng.");

            ban.TrangThai = "Trống";
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã bật hiển thị bàn." });
        }

        [HttpGet("danh-sach-pos")]
        public async Task<IActionResult> GetDanhSachPos([FromQuery] int chiNhanhId)
        {
            int cuaHangId = GetCuaHangId();
            var bans = await _context.Bans
                .Include(b => b.KhuVuc)
                .Where(b => b.KhuVuc!.ChiNhanhId == chiNhanhId && b.CuaHangId == cuaHangId && b.TrangThai != TrangThaiAn)
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
        [Authorize(Roles = "ChuCuaHang,QuanLy,Admin")]
        [HttpPost("tao-nhanh")]
        public async Task<IActionResult> TaoBanNhanh([FromBody] TaoBanNhanhDto request)
        {
            int cuaHangId = GetCuaHangId();
            var khuVucHopLe = await _context.KhuVucs
                .AnyAsync(kv => kv.Id == request.KhuVucId && kv.CuaHangId == cuaHangId);
            if (!khuVucHopLe)
                return BadRequest("Khu vực không thuộc cửa hàng hiện tại.");

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

            int daTao = 0;
            for (int i = 1; i <= request.SoLuong; i++)
            {
                var tenBanMoi = $"Bàn {soLonNhat + i}";
                bool isTrungTen = await _context.Bans.AnyAsync(b =>
                    b.CuaHangId == cuaHangId &&
                    b.KhuVucId == request.KhuVucId &&
                    b.TenBan.ToLower() == tenBanMoi.ToLower());
                if (isTrungTen)
                    continue;

                _context.Bans.Add(new Ban
                {
                    CuaHangId = cuaHangId,
                    KhuVucId = request.KhuVucId,
                    TenBan = tenBanMoi,
                    TrangThai = "Trống"
                });
                daTao++;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Đã tạo thành công {daTao} bàn!" });
        }

    }
}