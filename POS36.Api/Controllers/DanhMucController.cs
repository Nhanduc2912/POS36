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
        private readonly IAuditService _audit;

        public DanhMucController(AppDbContext context, ICloudStorageService cloudStorage, IAuditService audit)
        {
            _context = context;
            _cloudStorage = cloudStorage;
            _audit = audit;
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

        // Copy lại hàm UploadImageAsync (giống bên SanPhamController) để dùng
        private async Task<string?> UploadImageAsync(IFormFile? file)
        {
            return await _cloudStorage.UploadImageAsync(file, "danh-muc");
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
            await _audit.GhiLog("Tao", $"Tạo danh mục [{newDanhMuc.Id}] {newDanhMuc.TenDanhMuc}", "/danh-muc");
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
            await _audit.GhiLog("Sua", $"Sửa danh mục [{id}] {danhMuc.TenDanhMuc}", $"/danh-muc/{id}");
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
            await _audit.GhiLog("Xoa", $"Xóa danh mục [{id}] {danhMuc.TenDanhMuc}", $"/danh-muc/{id}");
            return Ok(new { message = "Xóa danh mục thành công!" });
        }
    }
}