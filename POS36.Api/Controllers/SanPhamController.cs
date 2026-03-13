using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using POS36.Api.DTOs;
using POS36.Api.Models;
using System.Security.Claims;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SanPhamController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SanPhamController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCuaHangId()
        {
            var claim = User.FindFirst("CuaHangId");
            if (claim == null) throw new UnauthorizedAccessException("Token không hợp lệ");
            return int.Parse(claim.Value);
        }

        // 1. LẤY DANH SÁCH MÓN ĂN (Kèm theo tên danh mục cho dễ nhìn)
        [HttpGet]
        public async Task<IActionResult> GetSanPhams()
        {
            int cuaHangId = GetCuaHangId();

            // Dùng Include để lấy luôn thông tin bảng DanhMuc (JOIN bảng)
            var sanPhams = await _context.SanPhams
                .Include(s => s.DanhMuc)
                .Where(s => s.CuaHangId == cuaHangId)
                .Select(s => new
                {
                    s.Id,
                    s.TenSanPham,
                    s.GiaBan,
                    s.TrangThai,
                    TenDanhMuc = s.DanhMuc!.TenDanhMuc // Lấy tên danh mục ra luôn cho Frontend dễ hiển thị
                })
                .ToListAsync();

            return Ok(sanPhams);
        }

        // 2. THÊM MỚI MÓN ĂN
        [HttpPost]
        public async Task<IActionResult> CreateSanPham(SanPhamDto request)
        {
            int cuaHangId = GetCuaHangId();

            // Kiểm tra xem DanhMucId này có thật sự thuộc về quán này không (Chống hack)
            var checkDanhMuc = await _context.DanhMucs
                .AnyAsync(d => d.Id == request.DanhMucId && d.CuaHangId == cuaHangId);

            if (!checkDanhMuc)
            {
                return BadRequest("Danh mục không tồn tại hoặc không thuộc quyền quản lý của bạn!");
            }

            var newSanPham = new SanPham
            {
                CuaHangId = cuaHangId,
                DanhMucId = request.DanhMucId,
                TenSanPham = request.TenSanPham,
                GiaBan = request.GiaBan,
                TrangThai = true // Mặc định vừa thêm là được bán luôn
            };

            _context.SanPhams.Add(newSanPham);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Thêm món ăn thành công!", id = newSanPham.Id });
        }
    }
}