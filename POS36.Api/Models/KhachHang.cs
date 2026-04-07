using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace POS36.Api.Models
{
    public class KhachHang
    {
        [Key] public int Id { get; set; }
        public int CuaHangId { get; set; }

        public string TenKhachHang { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? DiaChi { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? GhiChu { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // === TÍCH ĐIỂM (THIẾT KẾ 2 CỘT) ===
        // Chỉ CỘNG thêm, không bao giờ trừ → dùng để xét hạng thành viên (Đồng/Bạc/Vàng)
        public int TongDiemTichLuy { get; set; } = 0;
        // Điểm dùng để thanh toán/quy đổi, có thể CỘNG và bị TRỪ
        public int DiemHienTai { get; set; } = 0;

        [ForeignKey("CuaHangId")]
        public virtual CuaHang? CuaHang { get; set; }
    }
}