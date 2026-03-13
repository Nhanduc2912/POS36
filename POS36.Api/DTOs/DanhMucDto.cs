using System.ComponentModel.DataAnnotations;

namespace POS36.Api.DTOs
{
    public class DanhMucDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên danh mục!")]
        public string TenDanhMuc { get; set; } = string.Empty;
    }
}