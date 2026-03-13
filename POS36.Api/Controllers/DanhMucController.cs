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
    [Authorize] // BẮT BUỘC: Đánh dấu API này phải có Token (đã đăng nhập) mới được gọi
    public class DanhMucController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DanhMucController(AppDbContext context)
        {
            _context = context;
        }

        // Hàm hỗ trợ: Lấy CuaHangId từ Token đang đăng nhập
        private int GetCuaHangId()
        {
            var claim = User.FindFirst("CuaHangId");
            if (claim == null) throw new UnauthorizedAccessException("Token không hợp lệ hoặc thiếu CuaHangId");
            return int.Parse(claim.Value);
        }

        // 1. LẤY DANH SÁCH DANH MỤC CỦA QUÁN NÀY
        [HttpGet]
        public async Task<IActionResult> GetDanhMucs()
        {
            int cuaHangId = GetCuaHangId();
            // Chỉ query ra danh mục của đúng cửa hàng đang đăng nhập
            var danhMucs = await _context.DanhMucs
                .Where(d => d.CuaHangId == cuaHangId)
                .Select(d => new { d.Id, d.TenDanhMuc }) // Ẩn bớt thông tin thừa
                .ToListAsync();

            return Ok(danhMucs);
        }

        // 2. THÊM MỚI DANH MỤC
        [HttpPost]
        public async Task<IActionResult> CreateDanhMuc(DanhMucDto request)
        {
            int cuaHangId = GetCuaHangId();

            var newDanhMuc = new DanhMuc
            {
                CuaHangId = cuaHangId, // Tự động gán cho chủ quán đang thao tác
                TenDanhMuc = request.TenDanhMuc
            };

            _context.DanhMucs.Add(newDanhMuc);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Thêm danh mục thành công!", id = newDanhMuc.Id });
        }

        // 3. SỬA TÊN DANH MỤC
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDanhMuc(int id, DanhMucDto request)
        {
            int cuaHangId = GetCuaHangId();

            var danhMuc = await _context.DanhMucs
                .FirstOrDefaultAsync(d => d.Id == id && d.CuaHangId == cuaHangId);

            if (danhMuc == null) return NotFound("Không tìm thấy danh mục này!");

            danhMuc.TenDanhMuc = request.TenDanhMuc;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật thành công!" });
        }

        // 4. XÓA DANH MỤC
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDanhMuc(int id)
        {
            int cuaHangId = GetCuaHangId();

            var danhMuc = await _context.DanhMucs
                .FirstOrDefaultAsync(d => d.Id == id && d.CuaHangId == cuaHangId);

            if (danhMuc == null) return NotFound("Không tìm thấy danh mục này!");

            // Thực tế hệ thống lớn ít khi XÓA CỨNG, họ thường dùng cờ "IsDeleted = true" (Xóa mềm)
            // Nhưng tạm thời ở đây mình cứ cho xóa thẳng để dễ test
            _context.DanhMucs.Remove(danhMuc);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa danh mục thành công!" });
        }
    }
}