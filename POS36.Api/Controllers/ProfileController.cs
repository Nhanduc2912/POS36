using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using System.Security.Claims;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProfileController(AppDbContext context)
        {
            _context = context;
        }

        // Hàm lấy ID của người đang đăng nhập từ Token
        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("Id");
            if (claim == null) throw new UnauthorizedAccessException("Không xác định được danh tính");
            return int.Parse(claim.Value);
        }

        // =================================================================
        // 1. API TRẢ VỀ THÔNG TIN HIỆN TẠI (JOIN BẢNG TAIKHOAN VÀ NHANVIEN)
        // =================================================================
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            int userId = GetCurrentUserId();

            // Lấy Tài Khoản kèm theo thông tin Nhân Viên tương ứng
            var user = await _context.TaiKhoans
                .Include(t => t.NhanVien)
                .FirstOrDefaultAsync(t => t.Id == userId);

            if (user == null) return NotFound("Tài khoản không tồn tại");

            // Map đúng tên cột trong Database sếp chụp sang biến cho Frontend
            return Ok(new
            {
                username = user.TenDangNhap,
                phone = user.NhanVien?.SoDienThoai ?? "Chưa cập nhật",
                fullName = user.NhanVien?.TenNhanVien ?? "Admin/Chưa có tên",
                email = user.NhanVien?.Email ?? ""
            });
        }

        // =================================================================
        // 2. API CẬP NHẬT TÊN VÀ EMAIL (SỬA DỮ LIỆU BÊN BẢNG NHANVIEN)
        // =================================================================
        [HttpPut("update-info")]
        public async Task<IActionResult> UpdateInfo([FromBody] UpdateProfileDto dto)
        {
            int userId = GetCurrentUserId();

            var user = await _context.TaiKhoans
                .Include(t => t.NhanVien)
                .FirstOrDefaultAsync(t => t.Id == userId);

            if (user == null) return NotFound("Tài khoản không tồn tại");

            // Kiểm tra xem Tài khoản này đã có profile Nhân viên chưa
            if (user.NhanVien != null)
            {
                user.NhanVien.TenNhanVien = dto.FullName;
                user.NhanVien.Email = dto.Email;
            }
            else
            {
                // Nếu chưa có (VD tài khoản root admin), sếp có thể xử lý báo lỗi hoặc tự tạo mới.
                // Ở đây tạm thời báo lỗi để đảm bảo tính toàn vẹn dữ liệu
                return BadRequest("Tài khoản này chưa được liên kết với hồ sơ Nhân Viên nào!");
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công" });
        }

        // =================================================================
        // 3. API ĐỔI MẬT KHẨU BẢO MẬT (ĐÃ UPDATE BCRYPT)
        // =================================================================
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            int userId = GetCurrentUserId();
            var user = await _context.TaiKhoans.FindAsync(userId);
            if (user == null) return NotFound();

            // 1. DÙNG BCRYPT ĐỂ KIỂM TRA MẬT KHẨU CŨ (Mật khẩu gõ vào vs Mật khẩu đã mã hóa trong DB)
            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.MatKhauHash))
            {
                return BadRequest("Mật khẩu hiện tại không chính xác!");
            }

            // 2. BĂM (HASH) MẬT KHẨU MỚI TRƯỚC KHI LƯU VÀO DATABASE
            user.MatKhauHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Đổi mật khẩu thành công! Hãy ghi nhớ mật khẩu mới nhé." });
        }
    }

    // Các class DTO chứa dữ liệu từ Vue gửi lên giữ nguyên
    public class UpdateProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class ChangePasswordDto
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}