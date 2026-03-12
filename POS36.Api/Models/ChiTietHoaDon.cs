using System.ComponentModel.DataAnnotations;

namespace POS36.Api.Models
{
    public class ChiTietHoaDon
    {
        [Key]
        public int Id { get; set; }
        public int HoaDonId { get; set; }
        public int SanPhamId { get; set; }

        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public string GhiChu { get; set; } = string.Empty; // Ít đá, không hành...
        public string TrangThaiMon { get; set; } = "Chờ chế biến"; // Chờ chế biến, Đang làm, Đã xong, Đã giao

        public HoaDon? HoaDon { get; set; }
        public SanPham? SanPham { get; set; }
    }
}