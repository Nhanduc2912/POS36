namespace POS36.Api.DTOs
{
    public class NhanVienDto
    {
        // THÔNG TIN NHÂN SỰ
        public int? ChiNhanhId { get; set; }
        public string MaNhanVien { get; set; } = string.Empty;
        public string TenNhanVien { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;

        // THÔNG TIN TÀI KHOẢN (Có thể có hoặc không)
        public bool TaoTaiKhoan { get; set; } = false; // Checkbox xem có cấp quyền đăng nhập không
        public string? TenDangNhap { get; set; }
        public string? MatKhau { get; set; }
        public string? VaiTro { get; set; } // VD: "ThuNgan", "QuanLy"
    }
}