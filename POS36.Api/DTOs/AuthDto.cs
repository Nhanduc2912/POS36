using System.ComponentModel.DataAnnotations;

namespace POS36.Api.DTOs
{
    // Dữ liệu từ form Đăng ký
    public class RegisterDto
    {
        public string TenDangNhap { get; set; } = string.Empty;
        public string MatKhau { get; set; } = string.Empty;
        public string TenCuaHang { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty; // ĐÃ THÊM
    }

    // Dữ liệu từ form Đăng nhập
    public class LoginDto
    {
        public string TenDangNhap { get; set; } = string.Empty;
        public string MatKhau { get; set; } = string.Empty;
    }

    // Dữ liệu từ form Quên mật khẩu (Bước 1)
    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty; // Đổi từ TenDangNhap sang Email
    }

    // Dữ liệu từ form Nhập OTP và Đổi mật khẩu (Bước 2)
    public class ResetPasswordRequest
    {
        public string TenDangNhap { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
        public string MatKhauMoi { get; set; } = string.Empty;
    }
}