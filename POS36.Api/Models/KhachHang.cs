using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace POS36.Api.Models
{
    public class KhachHang
    {
        [Key] public int Id { get; set; }
        public int CuaHangId { get; set; }

        public string TenKhachHang { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public int DiemTichLuy { get; set; } = 0; // 1 điểm = 1000đ chẳng hạn
        [ForeignKey("CuaHangId")]
        public virtual CuaHang? CuaHang { get; set; }
    }
}