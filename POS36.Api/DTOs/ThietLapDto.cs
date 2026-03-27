using System.ComponentModel.DataAnnotations;

namespace POS36.Api.DTOs
{
    public class ChiNhanhDto
    {
        [Required] public string TenChiNhanh { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;
    }

    public class KhuVucDto
    {
        [Required] public int ChiNhanhId { get; set; }
        [Required] public string TenKhuVuc { get; set; } = string.Empty;
    }

    public class BanDto
    {
        [Required] public int KhuVucId { get; set; }
        [Required] public string MaBan { get; set; } = string.Empty; // Mã để gõ cho nhanh (VD: B01)
        [Required] public string TenBan { get; set; } = string.Empty; // Tên hiển thị (VD: Bàn số 1)
    }
    public class ThietLapDataDto
    {
        [Required] public string MaThietLap { get; set; } = string.Empty;
        public string DuLieu { get; set; } = string.Empty;
    }
}