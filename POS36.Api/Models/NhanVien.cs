using System.ComponentModel.DataAnnotations;

namespace POS36.Api.Models
{
    public class NhanVien
    {
        [Key] public int Id { get; set; }
        public int CuaHangId { get; set; }
        public int? ChiNhanhId { get; set; }

        public string MaNhanVien { get; set; } = string.Empty;
        public string TenNhanVien { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string? Email { get; set; }

        // THÔNG TIN HỒ SƠ PHÁP LÝ (Theo yêu cầu bắt buộc)
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
        public DateTime? NgayDongY { get; set; }

        // === XÓA MỀM ===
        public bool IsDeleted { get; set; } = false;
        public DateTime? NgayXoa { get; set; }
        public string? NguoiXoa { get; set; }

        public ChiNhanh? ChiNhanh { get; set; }
    }
}