using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS36.Api.Models
{
    /// <summary>
    /// Lịch sử mua/gia hạn gói dịch vụ của từng cửa hàng.
    /// Mỗi lần mua gói tạo 1 record mới để theo dõi toàn bộ lịch sử thanh toán.
    /// </summary>
    public class LichSuDangKy
    {
        [Key] public int Id { get; set; }
        public int CuaHangId { get; set; }
        public int GoiDichVuId { get; set; }

        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public decimal SoTienThanhToan { get; set; }

        // ChoThanhToan | DaThanhToan | DaHuy
        public string TrangThai { get; set; } = "ChoThanhToan";

        public string? PhuongThucThanhToan { get; set; }       // "SePay", "ChuyenKhoan", "ThuCong"
        public string? MaGiaoDich { get; set; }                 // Mã nội dung CK: POS36G{Id}
        public string? GhiChu { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime? NgayThanhToan { get; set; }
        public string? NguoiDuyet { get; set; }                 // SuperAdmin nào duyệt (nếu thủ công)

        // Navigation
        [ForeignKey("CuaHangId")]
        public CuaHang? CuaHang { get; set; }

        [ForeignKey("GoiDichVuId")]
        public GoiDichVu? GoiDichVu { get; set; }
    }
}
