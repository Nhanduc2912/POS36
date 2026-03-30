using System.ComponentModel.DataAnnotations;
// THÊM DÒNG NÀY ĐỂ C# HIỂU CHỮ [ForeignKey]
using System.ComponentModel.DataAnnotations.Schema;
namespace POS36.Api.Models
{
    public class ThietLap
    {
        [Key]
        public int Id { get; set; }
        public int CuaHangId { get; set; }
        public string MaThietLap { get; set; } = string.Empty; // Chứa Key (Vd: "BankConfig", "PrintTemplate")
        public string DuLieu { get; set; } = string.Empty;     // Chứa Value (JSON hoặc HTML)
                                                               // Thêm 2 dòng này vào dưới cùng của class PhieuThuChi và ThietLap
        [ForeignKey("CuaHangId")]
        public virtual CuaHang? CuaHang { get; set; }

        [ForeignKey("ChiNhanhId")] // (Chỉ thêm dòng này cho PhieuThuChi)
        public virtual ChiNhanh? ChiNhanh { get; set; }
    }
}