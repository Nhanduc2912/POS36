using System.ComponentModel.DataAnnotations;

namespace POS36.Api.Models
{
    public class HoaDon
    {
        [Key]
        public int Id { get; set; }
        public int CuaHangId { get; set; }
        public int ChiNhanhId { get; set; }
        public int BanId { get; set; }
        public int? NhanVienId { get; set; } // Ai mở bàn
                                             // Thêm cột này vào class HoaDon
        public string PhuongThucThanhToan { get; set; } = "Chưa thanh toán";
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime? NgayThanhToan { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; } = "Đang phục vụ"; // Đang phục vụ, Đã thanh toán, Hủy

        public Ban? Ban { get; set; }
        public ICollection<ChiTietHoaDon>? ChiTietHoaDons { get; set; }
    }
}