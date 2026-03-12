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

        public CuaHang? CuaHang { get; set; }
        public ChiNhanh? ChiNhanh { get; set; }
        public NhanVien? NhanVien { get; set; } // Navigation
    }
}