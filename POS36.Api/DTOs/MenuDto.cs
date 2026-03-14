using System.ComponentModel.DataAnnotations;

namespace POS36.Api.DTOs
{
    public class DanhMucDto
    {
        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        public string TenDanhMuc { get; set; } = string.Empty;
    }

    public class SanPhamDto
    {
        [Required] public int DanhMucId { get; set; }

        [Required(ErrorMessage = "Tên món không được để trống")]
        public string TenSanPham { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn hoặc bằng 0")]
        public decimal GiaBan { get; set; }

        // Trạng thái: true = Đang bán, false = Tạm hết / Ngừng bán
        public bool TrangThai { get; set; } = true;
    }
}