namespace POS36.Api.DTOs
{
    public class BaoCaoTongQuanDto
    {
        public decimal TongDoanhThu { get; set; }
        public int TongSoDonHang { get; set; }
        public List<ThongKePhuongThucDto> DoanhThuTheoPhuongThuc { get; set; } = new List<ThongKePhuongThucDto>();
        public List<MonBanChayDto> TopMonBanChay { get; set; } = new List<MonBanChayDto>();
    }

    public class ThongKePhuongThucDto
    {
        public string PhuongThuc { get; set; } = string.Empty;
        public decimal SoTien { get; set; }
    }

    public class MonBanChayDto
    {
        public string TenSanPham { get; set; } = string.Empty;
        public int SoLuongDaBan { get; set; }
    }
}