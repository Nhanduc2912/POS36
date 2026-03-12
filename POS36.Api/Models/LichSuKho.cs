using System.ComponentModel.DataAnnotations;

namespace POS36.Api.Models
{
    public class LichSuKho
    {
        [Key] public int Id { get; set; }
        public int ChiNhanhId { get; set; }
        public int SanPhamId { get; set; }

        public string LoaiGiaoDich { get; set; } = string.Empty; // Nhập hàng, Bán hàng, Xuất hủy...
        public int SoLuong { get; set; } // Có thể là số âm (xuất) hoặc dương (nhập)
        public DateTime NgayThucHien { get; set; } = DateTime.Now;
        public string GhiChu { get; set; } = string.Empty;

        public ChiNhanh? ChiNhanh { get; set; }
        public SanPham? SanPham { get; set; }
    }
}