using System.ComponentModel.DataAnnotations;

namespace POS36.Api.DTOs
{
    public class TaoDonHangDto
    {
        [Required] public int BanId { get; set; }

        // Danh sách các món khách gọi
        [Required] public List<ChiTietOrderDto> DanhSachMon { get; set; } = new List<ChiTietOrderDto>();
    }

    public class ChiTietOrderDto
    {
        [Required] public int SanPhamId { get; set; }
        [Required] public int SoLuong { get; set; }
        public string GhiChu { get; set; } = string.Empty; // VD: "Ít đá, nhiều đường"
    }

    public class ChuyenBanDto
    {
        [Required] public int TuBanId { get; set; }
        [Required] public int DenBanId { get; set; }
    }

    public class GhepBanDto
    {
        [Required] public int TuBanId { get; set; }
        [Required] public int DenBanId { get; set; }
    }
}