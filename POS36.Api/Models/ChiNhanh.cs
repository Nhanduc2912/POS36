using System.ComponentModel.DataAnnotations;

namespace POS36.Api.Models
{
    public class ChiNhanh
    {
        [Key]
        public int Id { get; set; }
        public int CuaHangId { get; set; }
        public string TenChiNhanh { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;

        // === XÓA MỀM ===
        public bool IsDeleted { get; set; } = false;
        public DateTime? NgayXoa { get; set; }
        public string? NguoiXoa { get; set; }

        public CuaHang? CuaHang { get; set; }
    }
}