using System.ComponentModel.DataAnnotations;

namespace POS36.Api.Models
{
    public class DanhMuc
    {
        [Key]
        public int Id { get; set; }
        public int CuaHangId { get; set; } // Danh mục này của chủ quán nào

        public string TenDanhMuc { get; set; } = string.Empty;
        public string? HinhAnh { get; set; } // Dấu ? nghĩa là cho phép null (không bắt buộc phải có ảnh)

        public ICollection<SanPham>? SanPhams { get; set; }
    }
}