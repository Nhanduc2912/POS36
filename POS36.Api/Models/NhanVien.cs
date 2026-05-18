using System.ComponentModel.DataAnnotations;

namespace POS36.Api.Models
{
    public class NhanVien
    {
        [Key] public int Id { get; set; }
        public int CuaHangId { get; set; }
        public int? ChiNhanhId { get; set; }

        public string MaNhanVien { get; set; } = string.Empty;
        public string TenNhanVien { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string? Email { get; set; }

        // === XÓA MỀM ===
        public bool IsDeleted { get; set; } = false;
        public DateTime? NgayXoa { get; set; }
        public string? NguoiXoa { get; set; }

        public ChiNhanh? ChiNhanh { get; set; }
    }
}