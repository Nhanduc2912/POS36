using System.ComponentModel.DataAnnotations;

namespace POS36.Api.Models
{
    public class SanPham
    {
        [Key] public int Id { get; set; }
        public int CuaHangId { get; set; }
        public int DanhMucId { get; set; }
        public string TenSanPham { get; set; } = string.Empty;
        public decimal GiaBan { get; set; }
        public bool TrangThai { get; set; } = true;

        // Lưu đường dẫn ảnh, ví dụ: /images/products/lau-thai.jpg
        public string? HinhAnh { get; set; }

        // FEAT-2: Ngưỡng cảnh báo tồn kho riêng cho từng sản phẩm
        // Default = 5 để backward compatible với dữ liệu cũ
        public int NgưỡngCanhBao { get; set; } = 5;

        public DanhMuc? DanhMuc { get; set; }
    }
}