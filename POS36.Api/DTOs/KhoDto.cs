using System.ComponentModel.DataAnnotations;

namespace POS36.Api.DTOs
{
    public class TaoPhieuNhapDto
    {
        [Required] public int ChiNhanhId { get; set; }
        public string GhiChu { get; set; } = string.Empty;

        [Required]
        public List<ChiTietNhapDto> DanhSachNhap { get; set; } = new List<ChiTietNhapDto>();
    }

    public class ChiTietNhapDto
    {
        [Required] public int SanPhamId { get; set; }
        [Required] public int SoLuong { get; set; }
        [Required] public decimal DonGiaNhap { get; set; }
    }
}