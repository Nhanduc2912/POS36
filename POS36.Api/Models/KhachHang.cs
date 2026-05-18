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
        public int TongDiemTichLuy { get; set; } = 0;
        public int DiemHienTai { get; set; } = 0;

        // === XÓA MỀM ===
        public bool IsDeleted { get; set; } = false;
        public DateTime? NgayXoa { get; set; }
        public string? NguoiXoa { get; set; }

        [ForeignKey("CuaHangId")]
        public virtual CuaHang? CuaHang { get; set; }
    }
}