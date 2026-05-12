using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS36.Api.Models
{
    /// <summary>
    /// Ghi nhận lượt truy cập hệ thống phục vụ Analytics cho SuperAdmin.
    /// </summary>
    public class LuotTruyCap
    {
        [Key] public int Id { get; set; }
        public int? CuaHangId { get; set; }        // Null = khách vãng lai (Landing Page)
        public int? TaiKhoanId { get; set; }

        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? ThietBi { get; set; }       // "Mobile", "Desktop"
        public DateTime ThoiGian { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("CuaHangId")]
        public CuaHang? CuaHang { get; set; }
    }
}
