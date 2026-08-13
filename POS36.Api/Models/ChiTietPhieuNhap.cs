using System.ComponentModel.DataAnnotations;

namespace POS36.Api.Models
{
    public class ChiTietPhieuNhap
    {
        [Key] public int Id { get; set; }
        public int PhieuNhapId { get; set; }
        public int NguyenVatLieuId { get; set; }

        public decimal SoLuong { get; set; }
        public decimal DonGiaNhap { get; set; } // Giá vốn để sau này tính lãi/lỗ
        
        public DateTime? NgayHetHan { get; set; } // Thêm Hạn sử dụng (Lô hàng)

        public PhieuNhap? PhieuNhap { get; set; }
        public NguyenVatLieu? NguyenVatLieu { get; set; }
    }
}