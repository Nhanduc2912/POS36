using Microsoft.AspNetCore.Http;

namespace POS36.Api.DTOs
{
    // DTO dùng cho Thêm/Sửa danh mục có kèm Ảnh
    public class CreateDanhMucDto
    {
        public string TenDanhMuc { get; set; } = string.Empty;
        public IFormFile? HinhAnhFile { get; set; }
    }
}