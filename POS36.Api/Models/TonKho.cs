using System.ComponentModel.DataAnnotations;

namespace POS36.Api.Models
{
    public class TonKho
    {
        [Key]
        public int Id { get; set; }
        public int ChiNhanhId { get; set; }
        public int NguyenVatLieuId { get; set; }

        public decimal SoLuong { get; set; } // Số lượng tồn của lô này

        public DateTime? NgayHetHan { get; set; } // Hạn sử dụng (nếu null thì coi như vô hạn)

        public ChiNhanh? ChiNhanh { get; set; }
        public NguyenVatLieu? NguyenVatLieu { get; set; }
    }
}