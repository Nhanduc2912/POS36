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

    // DTO cho từng dòng nguyên vật liệu trong phiếu nhập
    public class ChiTietPhieuNhapDto
    {
        public int NguyenVatLieuId { get; set; }
        public decimal SoLuong { get; set; }
        public decimal DonGiaNhap { get; set; } // Giá gốc lúc nhập vào
        public DateTime? NgayHetHan { get; set; } // Hạn sử dụng của lô này
    }
}