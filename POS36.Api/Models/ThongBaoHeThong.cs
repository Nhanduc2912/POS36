using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS36.Api.Models
{
    /// <summary>
    /// Thông báo từ SuperAdmin gửi đến các cửa hàng.
    /// CuaHangId = null → broadcast cho tất cả.
    /// </summary>
    public class ThongBaoHeThong
    {
        [Key] public int Id { get; set; }
        public int? CuaHangId { get; set; }         // Null = gửi cho tất cả

        public string TieuDe { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        // ThongTin | CanhBao | KhanCap
        public string LoaiThongBao { get; set; } = "ThongTin";
        public bool DaDoc { get; set; } = false;
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("CuaHangId")]
        public CuaHang? CuaHang { get; set; }
    }
}
