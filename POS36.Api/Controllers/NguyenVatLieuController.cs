using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using POS36.Api.Models;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NguyenVatLieuController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NguyenVatLieuController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCuaHangId()
        {
            var claim = User.FindFirst("CuaHangId");
            if (claim == null) throw new UnauthorizedAccessException("Token không hợp lệ");
            return int.Parse(claim.Value);
        }

        [HttpGet]
        public async Task<IActionResult> GetDanhSach([FromQuery] string? keyword, [FromQuery] int? danhMucId, [FromQuery] int? chiNhanhId)
        {
            var query = _context.NguyenVatLieus
                .Include(n => n.DanhMucNguyenVatLieu)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.TenNguyenVatLieu.Contains(keyword));
            }

            if (danhMucId.HasValue && danhMucId.Value > 0)
            {
                query = query.Where(x => x.DanhMucNguyenVatLieuId == danhMucId.Value);
            }

            var branchId = chiNhanhId.GetValueOrDefault();
            var branchClaim = User.FindFirst("ChiNhanhId");
            if (branchClaim != null)
            {
                branchId = int.Parse(branchClaim.Value);
            }

            var data = await query.Select(x => new
            {
                x.Id,
                x.CuaHangId,
                x.DanhMucNguyenVatLieuId,
                x.TenNguyenVatLieu,
                x.DonViTinh,
                x.NguongCanhBao,
                x.SoNgayCanhBaoHetHan,
                x.TrangThai,
                x.GiaVonHienTai,
                DanhMucNguyenVatLieu = x.DanhMucNguyenVatLieu,
                TonKho = _context.TonKhos
                    .Where(t => t.NguyenVatLieuId == x.Id && (branchId == 0 || t.ChiNhanhId == branchId))
                    .Sum(t => t.SoLuong)
            }).ToListAsync();

            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetChiTiet(int id)
        {
            var nvl = await _context.NguyenVatLieus.FindAsync(id);
            if (nvl == null) return NotFound("Nguyên vật liệu không tồn tại.");
            return Ok(nvl);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NguyenVatLieu nvl)
        {
            nvl.CuaHangId = GetCuaHangId();
            _context.NguyenVatLieus.Add(nvl);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Thêm thành công!", id = nvl.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] NguyenVatLieu nvlUpdate)
        {
            var nvl = await _context.NguyenVatLieus.FindAsync(id);
            if (nvl == null) return NotFound("Nguyên vật liệu không tồn tại.");

            nvl.TenNguyenVatLieu = nvlUpdate.TenNguyenVatLieu;
            nvl.DonViTinh = nvlUpdate.DonViTinh;
            nvl.NguongCanhBao = nvlUpdate.NguongCanhBao;
            nvl.SoNgayCanhBaoHetHan = nvlUpdate.SoNgayCanhBaoHetHan;
            nvl.TrangThai = nvlUpdate.TrangThai;
            nvl.DanhMucNguyenVatLieuId = nvlUpdate.DanhMucNguyenVatLieuId;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var nvl = await _context.NguyenVatLieus.FindAsync(id);
            if (nvl == null) return NotFound("Nguyên vật liệu không tồn tại.");

            // Kiểm tra xem đã có DinhLuong hay TonKho nào sử dụng chưa
            var isUsedInTonKho = await _context.TonKhos.AnyAsync(t => t.NguyenVatLieuId == id);
            var isUsedInDinhLuong = await _context.DinhLuongs.AnyAsync(d => d.NguyenVatLieuId == id);

            if (isUsedInTonKho || isUsedInDinhLuong)
            {
                return BadRequest("Nguyên vật liệu đang được sử dụng trong Kho hoặc Công thức, không thể xóa!");
            }

            _context.NguyenVatLieus.Remove(nvl);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa thành công!" });
        }

        [HttpGet("canhbao-hethan")]
        public async Task<IActionResult> GetCanhBaoHetHan([FromQuery] int chiNhanhId)
        {
            var branchClaim = User.FindFirst("ChiNhanhId");
            if (branchClaim != null)
            {
                int userBranchId = int.Parse(branchClaim.Value);
                if (chiNhanhId > 0 && chiNhanhId != userBranchId)
                    return StatusCode(403, "Bạn không có quyền xem dữ liệu chi nhánh khác!");
                chiNhanhId = userBranchId;
            }

            var now = DateTime.Now;

            // Lấy các lô tồn kho của chi nhánh
            var query = _context.TonKhos
                .Include(t => t.NguyenVatLieu)
                .Where(t => t.ChiNhanhId == chiNhanhId && t.SoLuong > 0 && t.NgayHetHan != null);

            var danhSachCanhBao = await query.ToListAsync();

            // Lọc ra các lô sắp hết hạn
            var ketQua = danhSachCanhBao
                .Where(t => t.NguyenVatLieu != null && t.NgayHetHan.HasValue && (t.NgayHetHan.Value - now).TotalDays <= t.NguyenVatLieu.SoNgayCanhBaoHetHan)
                .Select(t => new
                {
                    TonKhoId = t.Id,
                    NguyenVatLieuId = t.NguyenVatLieuId,
                    TenNguyenVatLieu = t.NguyenVatLieu!.TenNguyenVatLieu,
                    SoLuong = t.SoLuong,
                    DonViTinh = t.NguyenVatLieu.DonViTinh,
                    NgayHetHan = t.NgayHetHan,
                    SoNgayConLai = t.NgayHetHan.HasValue ? Math.Round((t.NgayHetHan.Value - now).TotalDays, 0) : 0
                })
                .OrderBy(x => x.SoNgayConLai)
                .ToList();

            return Ok(ketQua);
        }

        /// <summary>
        /// API đồng bộ giá vốn (MAC) cho toàn bộ NVL từ lịch sử phiếu nhập.
        /// Chạy 1 lần khi mới nâng cấp, hoặc khi cần recalculate.
        /// </summary>
        [HttpPost("dong-bo-gia-von")]
        public async Task<IActionResult> DongBoGiaVon()
        {
            var cuaHangId = GetCuaHangId();
            var allNVL = await _context.NguyenVatLieus
                .Where(n => n.CuaHangId == cuaHangId)
                .ToListAsync();

            int updated = 0;
            foreach (var nvl in allNVL)
            {
                // Lấy tất cả phiếu nhập theo thứ tự thời gian
                var lichSuNhap = await _context.ChiTietPhieuNhaps
                    .Include(ct => ct.PhieuNhap)
                    .Where(ct => ct.NguyenVatLieuId == nvl.Id && ct.PhieuNhap != null)
                    .OrderBy(ct => ct.PhieuNhap!.NgayNhap)
                    .Select(ct => new { ct.SoLuong, ct.DonGiaNhap })
                    .ToListAsync();

                if (!lichSuNhap.Any())
                {
                    nvl.GiaVonHienTai = 0;
                    continue;
                }

                // Mô phỏng lại công thức MAC từ đầu
                decimal tongSL = 0;
                decimal tongGiaTri = 0;
                foreach (var lot in lichSuNhap)
                {
                    tongGiaTri = (tongSL * (tongSL > 0 ? tongGiaTri / tongSL : 0)) + (lot.SoLuong * lot.DonGiaNhap);
                    tongSL += lot.SoLuong;
                }

                nvl.GiaVonHienTai = tongSL > 0 ? Math.Round(tongGiaTri / tongSL, 2) : 0;
                updated++;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Đã đồng bộ giá vốn MAC cho {updated}/{allNVL.Count} nguyên vật liệu." });
        }
    }
}
