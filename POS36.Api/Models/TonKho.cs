using System.ComponentModel.DataAnnotations;

namespace POS36.Api.Models
{
    public class TonKho
    {
        [Key]
        public int Id { get; set; }
        public int ChiNhanhId { get; set; }
        public int SanPhamId { get; set; }

        public int SoLuong { get; set; } // Cứ bán 1 đơn thì trừ đi

        public ChiNhanh? ChiNhanh { get; set; }
        public SanPham? SanPham { get; set; }
    }
}