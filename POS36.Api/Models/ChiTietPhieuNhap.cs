using System.ComponentModel.DataAnnotations;

namespace POS36.Api.Models
{
    public class ChiTietPhieuNhap
    {
        [Key] public int Id { get; set; }
        public int PhieuNhapId { get; set; }
        public int SanPhamId { get; set; }

        public int SoLuong { get; set; }
        public decimal DonGiaNhap { get; set; } // Giá vốn để sau này tính lãi/lỗ

        public PhieuNhap? PhieuNhap { get; set; }
        public SanPham? SanPham { get; set; }
    }
}