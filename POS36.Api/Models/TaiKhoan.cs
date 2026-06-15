using System.ComponentModel.DataAnnotations;

namespace POS36.Api.Models
{
    public class TaiKhoan
    {
        [Key] public int Id { get; set; }
        public int CuaHangId { get; set; } 
        public int? ChiNhanhId { get; set; } 
        public int? NhanVienId { get; set; } // Bổ sung: Link tới nhân viên thực tế

        public string TenDangNhap { get; set; } = string.Empty;
        public string MatKhauHash { get; set; } = string.Empty;
        public string VaiTro { get; set; } = string.Empty; 

        // === SaaS: Thông tin liên lạc trực tiếp (không phụ thuộc bảng NhanVien) ===
        public string Email { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public DateTime? LanDangNhapCuoi { get; set; }

        // === Phân quyền Admin cho Thu ngân ===
        // Lưu danh sách quyền dạng chuỗi phân cách bởi dấu phẩy
        // Ví dụ: "view_orders,view_cashbook,view_daily_summary"
        // Chỉ có ý nghĩa khi VaiTro = "ThuNgan"
        public string? QuyenThuNgan { get; set; }

        public CuaHang? CuaHang { get; set; }
        public ChiNhanh? ChiNhanh { get; set; }
        public NhanVien? NhanVien { get; set; } // Navigation
    }
}