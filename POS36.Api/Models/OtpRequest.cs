using System;
using System.ComponentModel.DataAnnotations;

namespace POS36.Api.Models
{
    /// <summary>
    /// BUG #2 FIX: Lưu OTP vào DB thay vì RAM.
    /// Mỗi request có ExpiresAt để tự hết hạn sau 5 phút.
    /// </summary>
    public class OtpRequest
    {
        [Key]
        public int Id { get; set; }

        /// <summary>TenDangNhap của TaiKhoan cần reset mật khẩu</summary>
        public string TenDangNhap { get; set; } = string.Empty;

        /// <summary>Mã OTP 6 số được hash bằng bcrypt</summary>
        public string OtpHash { get; set; } = string.Empty;

        /// <summary>Thời điểm OTP hết hạn (mặc định 5 phút sau khi tạo)</summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>OTP đã được dùng chưa (tránh replay attack)</summary>
        public bool DaSDung { get; set; } = false;
    }
}
