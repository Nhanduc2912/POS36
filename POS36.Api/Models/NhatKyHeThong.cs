using System.ComponentModel.DataAnnotations;

namespace POS36.Api.Models
{
    /// <summary>
    /// Nhật ký hệ thống — ghi lại mọi thay đổi quan trọng của Super Admin
    /// </summary>
    public class NhatKyHeThong
    {
        [Key] public int Id { get; set; }

        [Required, MaxLength(50)]
        public string HanhDong { get; set; } = string.Empty;  // "Tao", "Sua", "Xoa", "KhoaCuaHang", "CauHinh"

        [Required, MaxLength(500)]
        public string MoTa { get; set; } = string.Empty;       // "Xóa cửa hàng #5 - Quán Café ABC"

        [MaxLength(500)]
        public string? UrlLienQuan { get; set; }               // "/super-admin/stores?id=5"

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        [MaxLength(100)]
        public string? NguoiThucHien { get; set; }

        public DateTime ThoiGian { get; set; } = DateTime.Now;

        public string? ChiTietJson { get; set; }               // JSON snapshot trước/sau thay đổi
    }
}
