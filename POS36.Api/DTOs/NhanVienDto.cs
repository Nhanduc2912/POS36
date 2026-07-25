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

        // THÔNG TIN HỒ SƠ PHÁP LÝ (Bắt buộc)
        public string Cccd { get; set; } = string.Empty;
        public DateTime? NgayCapCccd { get; set; }
        public string NoiCapCccd { get; set; } = string.Empty;
        public string GioiTinh { get; set; } = string.Empty;
        public DateTime? NgaySinh { get; set; }
        public string DiaChiThuongTru { get; set; } = string.Empty;
        public string DiaChiTamTru { get; set; } = string.Empty;
        public DateTime? NgayVaoLam { get; set; }

        // LIÊN HỆ KHẨN CẤP
        public string? NguoiLienHeKhanCap { get; set; }
        public string? SdtKhanCap { get; set; }
        public string? MoiQuanHeKhanCap { get; set; }

        // ĐỒNG Ý XỬ LÝ DỮ LIỆU CÁ NHÂN (NĐ 13/2023/NĐ-CP)
        public bool DongYXuLyDuLieu { get; set; } = false;

        // THÔNG TIN TÀI KHOẢN (Bắt buộc phải cấp khi thêm mới)
        public bool TaoTaiKhoan { get; set; } = true;
        public string? TenDangNhap { get; set; }
        public string? MatKhau { get; set; }
        public string? VaiTro { get; set; } // VD: "ThuNgan", "Order", "Bep"
    }

    // DTO cập nhật quyền Admin cho Thu ngân
    public class UpdateQuyenDto
    {
        // Danh sách quyền phân cách bởi dấu phẩy
        // VD: "view_orders,view_cashbook,view_daily_summary"
        // Để rỗng = thu hồi toàn bộ quyền
        public string? QuyenThuNgan { get; set; }
    }
}