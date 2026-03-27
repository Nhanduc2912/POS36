using System.ComponentModel.DataAnnotations;

namespace POS36.Api.Models
{
    public class ThietLap
    {
        [Key]
        public int Id { get; set; }
        public int CuaHangId { get; set; }
        public string MaThietLap { get; set; } = string.Empty; // Chứa Key (Vd: "BankConfig", "PrintTemplate")
        public string DuLieu { get; set; } = string.Empty;     // Chứa Value (JSON hoặc HTML)
    }
}