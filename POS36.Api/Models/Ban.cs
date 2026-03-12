using System.ComponentModel.DataAnnotations;

namespace POS36.Api.Models
{
    public class Ban
    {
        [Key] public int Id { get; set; }
        public int CuaHangId { get; set; }
        public int KhuVucId { get; set; }

        public string MaBan { get; set; } = string.Empty; // Bổ sung: B01, VIP1...
        public string TenBan { get; set; } = string.Empty;
        public string TrangThai { get; set; } = "Trống"; 

        public KhuVuc? KhuVuc { get; set; }
    }
}