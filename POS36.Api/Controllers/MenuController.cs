using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using POS36.Api.DTOs;
using POS36.Api.Models;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Bắt buộc phải có Token đăng nhập
    public class MenuController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MenuController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCuaHangId()
        {
            var claim = User.FindFirst("CuaHangId");
            if (claim == null) throw new UnauthorizedAccessException("Token không hợp lệ");
            return int.Parse(claim.Value);
        }

        // ==========================================
        // 1. TẠO DANH MỤC MỚI
        // ==========================================
        [HttpPost("danhmuc")]
        public async Task<IActionResult> CreateDanhMuc(DanhMucDto request)
        {
            var newDanhMuc = new DanhMuc
            {
                CuaHangId = GetCuaHangId(),
                TenDanhMuc = request.TenDanhMuc
            };

            _context.DanhMucs.Add(newDanhMuc);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Thêm danh mục thành công!", id = newDanhMuc.Id });
        }

        // ==========================================
        // 2. LẤY DANH SÁCH DANH MỤC
        // ==========================================
        [HttpGet("danhmuc")]
        public async Task<IActionResult> GetDanhMucs()
        {
            var danhMucs = await _context.DanhMucs
                .Where(d => d.CuaHangId == GetCuaHangId())
                .Select(d => new { d.Id, d.TenDanhMuc }) // Chỉ lấy những cột cần thiết cho nhẹ băng thông
                .ToListAsync();

            return Ok(danhMucs);
        }

        // ==========================================
        // 3. TẠO MÓN ĂN MỚI
        // ==========================================
        [HttpPost("sanpham")]
        public async Task<IActionResult> CreateSanPham(SanPhamDto request)
        {
            int cuaHangId = GetCuaHangId();

            // Ràng buộc bảo mật: Check xem Danh mục này có đúng là của Cửa hàng này không
            var checkDanhMuc = await _context.DanhMucs.AnyAsync(d => d.Id == request.DanhMucId && d.CuaHangId == cuaHangId);
            if (!checkDanhMuc) return BadRequest("Danh mục không tồn tại hoặc không hợp lệ!");

            var newSanPham = new SanPham
            {
                CuaHangId = cuaHangId,
                DanhMucId = request.DanhMucId,
                TenSanPham = request.TenSanPham,
                GiaBan = request.GiaBan,
                TrangThai = request.TrangThai
            };

            _context.SanPhams.Add(newSanPham);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Thêm món ăn thành công!", id = newSanPham.Id });
        }

        // ==========================================
        // 4. LẤY MENU MÓN ĂN (Dùng cho nhân viên Order)
        // ==========================================
        [HttpGet("sanpham")]
        public async Task<IActionResult> GetSanPhams()
        {
            // Kỹ thuật JOIN bảng SanPham với DanhMuc để lấy tên thay vì ID khô khan
            var sanPhams = await _context.SanPhams
                .Include(s => s.DanhMuc)
                .Where(s => s.CuaHangId == GetCuaHangId() && s.TrangThai == true) // Chỉ lấy các món Đang bán
                .Select(s => new
                {
                    s.Id,
                    s.TenSanPham,
                    s.GiaBan,
                    TenDanhMuc = s.DanhMuc!.TenDanhMuc // Lấy tên danh mục từ bảng nối
                })
                .ToListAsync();

            return Ok(sanPhams);
        }
    }
}