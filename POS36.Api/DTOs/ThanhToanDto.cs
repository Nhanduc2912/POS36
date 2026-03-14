using System.ComponentModel.DataAnnotations;

namespace POS36.Api.DTOs
{
    public class TaoThanhToanDto
    {
        [Required]
        public int HoaDonId { get; set; }

        [Required]
        public string PhuongThuc { get; set; } = "Tiền mặt"; // Hoặc "Chuyển khoản"

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0")]
        public decimal SoTienKhachDua { get; set; }
    }
}