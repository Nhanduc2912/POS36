using System.ComponentModel.DataAnnotations;

namespace POS36.Api.Models
{
    public class KhuVuc
    {
        [Key]
        public int Id { get; set; }
        public int CuaHangId { get; set; } // Thuộc chuỗi nào
        public int ChiNhanhId { get; set; } // Thuộc chi nhánh nào

        public string TenKhuVuc { get; set; } = string.Empty;

        public ChiNhanh? ChiNhanh { get; set; }
        public ICollection<Ban>? Bans { get; set; }
    }
}