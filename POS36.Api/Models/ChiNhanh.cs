using System.ComponentModel.DataAnnotations;

namespace POS36.Api.Models
{
    public class ChiNhanh
    {
        [Key]
        public int Id { get; set; }
        public int CuaHangId { get; set; } // Khóa ngoại tới CuaHang
        public string TenChiNhanh { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;

        public CuaHang? CuaHang { get; set; }
    }
}