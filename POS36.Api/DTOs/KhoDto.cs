namespace POS36.Api.DTOs
{
    // DTO hứng dữ liệu của cả 1 Phiếu Nhập
    public class TaoPhieuNhapDto
    {
        public int ChiNhanhId { get; set; } // Bắt buộc phải biết nhập cho chi nhánh nào
        public string GhiChu { get; set; } = string.Empty;
        public decimal TongTien { get; set; }

        // Danh sách các mặt hàng được nhập
        public List<ChiTietPhieuNhapDto> ChiTiets { get; set; } = new();
    }

    // DTO cho từng dòng sản phẩm trong phiếu nhập
    public class ChiTietPhieuNhapDto
    {
        public int SanPhamId { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGiaNhap { get; set; } // Giá gốc lúc nhập vào
    }
}