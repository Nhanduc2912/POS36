using System.ComponentModel.DataAnnotations;

namespace POS36.Api.Models
{
    public class DanhMuc
    {
        [Key]
        public int Id { get; set; }
        public int CuaHangId { get; set; } // Danh mục này của chủ quán nào
        
        public string TenDanhMuc { get; set; } = string.Empty;
        
        public ICollection<SanPham>? SanPhams { get; set; }
    }
}