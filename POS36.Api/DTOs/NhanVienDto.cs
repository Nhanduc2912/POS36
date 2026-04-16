namespace POS36.Api.DTOs
{
    public class NhanVienDto
    {
        // THÔNG TIN NHÂN SỰ
        public int? ChiNhanhId { get; set; }
        public string MaNhanVien { get; set; } = string.Empty;
        public string TenNhanVien { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string? Email { get; set; } // Có thể null

        // THÔNG TIN TÀI KHOẢN (Bắt buộc phải cấp khi thêm mới)
        public bool TaoTaiKhoan { get; set; } = true;
        public string? TenDangNhap { get; set; }
        public string? MatKhau { get; set; }
        public string? VaiTro { get; set; } // VD: "ThuNgan", "Order", "Bep"
    }
}