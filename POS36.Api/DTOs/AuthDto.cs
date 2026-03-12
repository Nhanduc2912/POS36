using System.ComponentModel.DataAnnotations;

namespace POS36.Api.DTOs
{
    // DTO dùng lúc người ta bấm Đăng ký
    public class RegisterDto
    {
        [Required] public string TenCuaHang { get; set; } = string.Empty;
        [Required] public string SoDienThoai { get; set; } = string.Empty;
        [Required] public string TenDangNhap { get; set; } = string.Empty;
        [Required] public string MatKhau { get; set; } = string.Empty;
    }

    // DTO dùng lúc người ta bấm Đăng nhập
    public class LoginDto
    {
        [Required] public string TenDangNhap { get; set; } = string.Empty;
        [Required] public string MatKhau { get; set; } = string.Empty;
    }
}