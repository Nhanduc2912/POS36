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
        // Dấu chấm hỏi (?) đằng sau chữ string báo cho C# và SQL biết: Cột này ĐƯỢC PHÉP NULL
        public string? Email { get; set; }

        public ChiNhanh? ChiNhanh { get; set; }
    }
}