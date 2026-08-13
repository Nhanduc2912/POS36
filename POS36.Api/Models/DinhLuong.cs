using System.ComponentModel.DataAnnotations;

namespace POS36.Api.Models
{
    public class DinhLuong
    {
        [Key]
        public int Id { get; set; }
        
        public int SanPhamId { get; set; }
        public int NguyenVatLieuId { get; set; }
        
        // Số lượng nguyên vật liệu tiêu hao cho 1 đơn vị sản phẩm
        // Ví dụ: 1 Cà phê sữa (SanPham) cần 20g (SoLuong) Cà phê hạt (NguyenVatLieu)
        public decimal SoLuong { get; set; }
        
        public SanPham? SanPham { get; set; }
        public NguyenVatLieu? NguyenVatLieu { get; set; }
    }
}
