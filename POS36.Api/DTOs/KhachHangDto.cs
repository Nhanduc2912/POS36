namespace POS36.Api.DTOs
{
    // Dùng cho Thêm mới / Sửa khách hàng
    public class KhachHangDto
    {
        public string TenKhachHang { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? DiaChi { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? GhiChu { get; set; }
    }
}
