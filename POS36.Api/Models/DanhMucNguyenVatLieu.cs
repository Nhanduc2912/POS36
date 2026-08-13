using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace POS36.Api.Models
{
    public class DanhMucNguyenVatLieu
    {
        [Key]
        public int Id { get; set; }

        public int CuaHangId { get; set; }

        [Required]
        [MaxLength(200)]
        public string TenDanhMuc { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? HinhAnh { get; set; }

        public bool TrangThai { get; set; } = true;

        [JsonIgnore]
        public ICollection<NguyenVatLieu> NguyenVatLieus { get; set; } = new List<NguyenVatLieu>();
    }
}
