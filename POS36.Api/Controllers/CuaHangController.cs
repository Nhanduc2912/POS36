using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using System.Security.Claims;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Bắt buộc đăng nhập
    public class CuaHangController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CuaHangController(AppDbContext context)
        {
            _context = context;
        }

        // Lấy CuaHangId từ Token (Lúc nãy đăng nhập anh em mình đã nhét sẵn vào rồi)
        private int GetCurrentCuaHangId()
        {
            var claim = User.FindFirst("CuaHangId");
            if (claim == null) throw new UnauthorizedAccessException("Không xác định được cửa hàng");
            return int.Parse(claim.Value);
        }

        // 1. API LẤY THÔNG TIN CỬA HÀNG
        [HttpGet("info")]
        public async Task<IActionResult> GetStoreInfo()
        {
            try
            {
                int cuaHangId = GetCurrentCuaHangId();
                var store = await _context.CuaHangs.FindAsync(cuaHangId);

                if (store == null) return NotFound("Không tìm thấy dữ liệu cửa hàng");

                return Ok(new
                {
                    tenCuaHang = store.TenCuaHang,
                    soDienThoai = store.SoDienThoai,
                    ngayDangKy = store.NgayDangKy
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // 2. API CẬP NHẬT THÔNG TIN CỬA HÀNG
        [HttpPut("update-info")]
        public async Task<IActionResult> UpdateStoreInfo([FromBody] UpdateStoreDto dto)
        {
            try
            {
                int cuaHangId = GetCurrentCuaHangId();
                var store = await _context.CuaHangs.FindAsync(cuaHangId);

                if (store == null) return NotFound("Không tìm thấy dữ liệu cửa hàng");

                // Cập nhật thông tin
                store.TenCuaHang = dto.TenCuaHang;
                store.SoDienThoai = dto.SoDienThoai;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Cập nhật thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi server: " + ex.Message);
            }
        }
    }

    // DTO để nhận cục dữ liệu từ Vue gửi lên
    public class UpdateStoreDto
    {
        public string TenCuaHang { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
    }
}