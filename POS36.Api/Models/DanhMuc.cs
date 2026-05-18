using System.ComponentModel.DataAnnotations;

namespace POS36.Api.Models
{
    public class DanhMuc
    {
        [Key]
        public int Id { get; set; }
        public int CuaHangId { get; set; }

        public string TenDanhMuc { get; set; } = string.Empty;
        public string? HinhAnh { get; set; }

        // === XÓA MỀM ===
        public bool IsDeleted { get; set; } = false;
        public DateTime? NgayXoa { get; set; }
        public string? NguoiXoa { get; set; }

        public ICollection<SanPham>? SanPhams { get; set; }
    }
}