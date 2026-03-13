using System.ComponentModel.DataAnnotations;

namespace POS36.Api.DTOs
{
    public class SanPhamDto
    {
        [Required(ErrorMessage = "Vui lòng chọn danh mục!")]
        public int DanhMucId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên món ăn!")]
        public string TenSanPham { get; set; } = string.Empty;

        [Required]
        public decimal GiaBan { get; set; }
    }
}