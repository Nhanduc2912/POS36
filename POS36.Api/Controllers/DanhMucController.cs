using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using POS36.Api.DTOs;
using POS36.Api.Models;
using System.Security.Claims;

using POS36.Api.Services;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // BẮT BUỘC: Đánh dấu API này phải có Token (đã đăng nhập) mới được gọi
    public class DanhMucController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ICloudStorageService _cloudStorage;

        public DanhMucController(AppDbContext context, ICloudStorageService cloudStorage)
        {
            _context = context;
            _cloudStorage = cloudStorage;
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

            var danhMucs = await _context.DanhMucs
                .Where(d => d.CuaHangId == cuaHangId && !d.IsDeleted)
                .Select(d => new { d.Id, d.TenDanhMuc, d.HinhAnh })
                .ToListAsync();

            return Ok(danhMucs);
        }
        // 2. THÊM MỚI DANH MỤC
        // DTO nhận dữ liệu Form của Danh mục
        public class CreateDanhMucDto
        {
            public string TenDanhMuc { get; set; } = string.Empty;
            public IFormFile? HinhAnhFile { get; set; }
        }

        // Hàm hỗ trợ UploadImageAsync sử dụng Cloud Storage / Local fallback
        private async Task<string?> UploadImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;
            return await _cloudStorage.UploadImageAsync(file, "pos36/categories");
        }

        // 2. THÊM MỚI DANH MỤC (Có Up Ảnh)
        // BUG #12 FIX: Chỉ ChuCuaHang mới được thêm danh mục
        [Authorize(Roles = "ChuCuaHang")]
        [HttpPost]
        public async Task<IActionResult> CreateDanhMuc([FromForm] CreateDanhMucDto request)
        {
            int cuaHangId = GetCuaHangId();
            string? hinhAnhPath = await UploadImageAsync(request.HinhAnhFile);

            var newDanhMuc = new DanhMuc
            {
                CuaHangId = cuaHangId,
                TenDanhMuc = request.TenDanhMuc,
                HinhAnh = hinhAnhPath // Gắn ảnh vào Database
            };

            _context.DanhMucs.Add(newDanhMuc);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Thêm danh mục thành công!", id = newDanhMuc.Id });
        }

        // 3. SỬA TÊN DANH MỤC
        // BUG #12 FIX: Chỉ ChuCuaHang mới được sửa danh mục
        [Authorize(Roles = "ChuCuaHang")]
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
        // BUG #12 FIX: Chỉ ChuCuaHang mới được xóa danh mục
        [Authorize(Roles = "ChuCuaHang")]
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