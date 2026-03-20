using Microsoft.AspNetCore.Http;

namespace POS36.Api.DTOs
{
    // DTO dùng cho Thêm/Sửa sản phẩm có kèm Ảnh
    public class CreateSanPhamDto
    {
        public int DanhMucId { get; set; }
        public string TenSanPham { get; set; } = string.Empty;
        public decimal GiaBan { get; set; }
        public IFormFile? HinhAnhFile { get; set; }
    }

    // DTO dùng cho việc Cập nhật giá bán siêu tốc
    public class UpdatePriceDto
    {
        public decimal GiaBan { get; set; }
    }
}