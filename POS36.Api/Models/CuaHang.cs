using System.ComponentModel.DataAnnotations;

namespace POS36.Api.Models
{
    public class CuaHang
    {
        [Key]
        public int Id { get; set; } // Đây chính là TenantId
        public string TenCuaHang { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        // Navigation properties
        public ICollection<ChiNhanh>? ChiNhanhs { get; set; }
        public ICollection<TaiKhoan>? TaiKhoans { get; set; }
    }
}