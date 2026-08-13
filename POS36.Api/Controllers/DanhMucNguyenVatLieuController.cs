using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using POS36.Api.Models;
using POS36.Api.Services;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DanhMucNguyenVatLieuController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ICloudStorageService _cloudStorage;

        public DanhMucNguyenVatLieuController(AppDbContext context, ICloudStorageService cloudStorage)
        {
            _context = context;
            _cloudStorage = cloudStorage;
        }

        private int GetCuaHangId()
        {
            var claim = User.FindFirst("CuaHangId");
            if (claim == null) throw new UnauthorizedAccessException("Token không hợp lệ");
            return int.Parse(claim.Value);
        }

        [HttpGet]
        public async Task<IActionResult> GetDanhMucs()
        {
            int cuaHangId = GetCuaHangId();
            var list = await _context.DanhMucNguyenVatLieus
                .Where(d => d.CuaHangId == cuaHangId && d.TrangThai == true)
                .Select(d => new
                {
                    d.Id,
                    d.TenDanhMuc,
                    d.HinhAnh
                })
                .ToListAsync();

            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] string TenDanhMuc, IFormFile? HinhAnhFile)
        {
            int cuaHangId = GetCuaHangId();

            var dm = new DanhMucNguyenVatLieu
            {
                CuaHangId = cuaHangId,
                TenDanhMuc = TenDanhMuc
            };

            if (HinhAnhFile != null)
            {
                var url = await _cloudStorage.UploadImageAsync(HinhAnhFile);
                dm.HinhAnh = url;
            }

            _context.DanhMucNguyenVatLieus.Add(dm);
            await _context.SaveChangesAsync();

            await _context.LogHoatDongAsync(int.Parse(User.FindFirst("ChiNhanhId")?.Value ?? "0"), "Danh mục Kho", $"Thêm danh mục NVL '{dm.TenDanhMuc}'");
            return Ok(dm);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] string TenDanhMuc, IFormFile? HinhAnhFile)
        {
            int cuaHangId = GetCuaHangId();
            var dm = await _context.DanhMucNguyenVatLieus
                .FirstOrDefaultAsync(d => d.Id == id && d.CuaHangId == cuaHangId);

            if (dm == null) return NotFound("Không tìm thấy danh mục");

            dm.TenDanhMuc = TenDanhMuc;

            if (HinhAnhFile != null)
            {
                dm.HinhAnh = await _cloudStorage.UploadImageAsync(HinhAnhFile);
            }

            await _context.SaveChangesAsync();
            await _context.LogHoatDongAsync(int.Parse(User.FindFirst("ChiNhanhId")?.Value ?? "0"), "Danh mục Kho", $"Cập nhật danh mục NVL '{dm.TenDanhMuc}'");
            return Ok(dm);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            int cuaHangId = GetCuaHangId();
            var dm = await _context.DanhMucNguyenVatLieus
                .FirstOrDefaultAsync(d => d.Id == id && d.CuaHangId == cuaHangId);

            if (dm == null) return NotFound("Không tìm thấy danh mục");

            bool hasItems = await _context.NguyenVatLieus.AnyAsync(n => n.DanhMucNguyenVatLieuId == id);
            if (hasItems) return BadRequest("Không thể xóa danh mục đang có nguyên vật liệu!");

            dm.TrangThai = false; // Soft delete
            await _context.SaveChangesAsync();

            await _context.LogHoatDongAsync(int.Parse(User.FindFirst("ChiNhanhId")?.Value ?? "0"), "Danh mục Kho", $"Xóa danh mục NVL '{dm.TenDanhMuc}'");
            return Ok(new { message = "Xóa thành công" });
        }
    }
}
