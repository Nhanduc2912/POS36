using System.ComponentModel.DataAnnotations;

namespace POS36.Api.Models
{
    public class CuaHang
    {
        [Key]
        public int Id { get; set; } // Đây chính là TenantId
        public string TenCuaHang { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        // === SaaS Subscription Fields ===
        public string? Email { get; set; }
        public string? DiaChi { get; set; }
        public string? LogoUrl { get; set; }
        // DungThu | HoatDong | ChiDoc | BiKhoa
        public string TrangThai { get; set; } = "DungThu";
        public DateTime NgayHetHan { get; set; } = DateTime.Now.AddDays(7);
        public string? GoiDichVu { get; set; } // MaGoi: "STARTER", "PRO", "ULTIMATE"
        public string? GhiChu { get; set; } // Ghi chú từ SuperAdmin

        // Navigation properties
        public ICollection<ChiNhanh>? ChiNhanhs { get; set; }
        public ICollection<TaiKhoan>? TaiKhoans { get; set; }
    }
}