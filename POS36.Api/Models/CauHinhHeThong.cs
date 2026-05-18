using System.ComponentModel.DataAnnotations;

namespace POS36.Api.Models
{
    /// <summary>
    /// Cấu hình hệ thống Super Admin (key-value store)
    /// NhomCauHinh: General | Payment | Email | System
    /// </summary>
    public class CauHinhHeThong
    {
        [Key] public int Id { get; set; }

        [Required, MaxLength(50)]
        public string NhomCauHinh { get; set; } = string.Empty;  // "General", "Payment", "Email"

        [Required, MaxLength(100)]
        public string MaKey { get; set; } = string.Empty;         // "SiteLogo", "PrimaryColor"

        public string GiaTri { get; set; } = string.Empty;        // Giá trị (có thể là JSON)

        [MaxLength(500)]
        public string? MoTa { get; set; }

        public DateTime NgayCapNhat { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string? NguoiCapNhat { get; set; }
    }
}
