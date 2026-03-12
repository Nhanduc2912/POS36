using System.ComponentModel.DataAnnotations;

namespace POS36.Api.Models
{
    public class ThanhToan
    {
        [Key] public int Id { get; set; }
        public int HoaDonId { get; set; }

        public string PhuongThuc { get; set; } = "Tiền mặt"; // Tiền mặt, Chuyển khoản, Quẹt thẻ
        public decimal SoTien { get; set; }
        public DateTime NgayThanhToan { get; set; } = DateTime.Now;

        public HoaDon? HoaDon { get; set; }
    }
}