using System;
// THÊM DÒNG NÀY ĐỂ C# HIỂU CHỮ [ForeignKey]
using System.ComponentModel.DataAnnotations.Schema;

namespace POS36.Api.Models
{
    public class PhieuThuChi
    {
        public int Id { get; set; }
        public int CuaHangId { get; set; }
        public int ChiNhanhId { get; set; }

        public string MaChungTu { get; set; } = string.Empty; // PT... (Phiếu thu) hoặc PC... (Phiếu chi)
        public string LoaiPhieu { get; set; } = "Thu"; // "Thu" hoặc "Chi"
        public string PhuongThuc { get; set; } = "Tiền mặt"; // Tiền mặt, Chuyển khoản, Thẻ...

        public string NguoiNopNhan { get; set; } = string.Empty; // Khách hàng, NCC, Nhân viên...
        public string HangMuc { get; set; } = "Thu tiền bán hàng"; // Phân loại
        public string LyDo { get; set; } = string.Empty;

        public double GiaTri { get; set; }
        public DateTime NgayGiaoDich { get; set; }
        public string NguoiTao { get; set; } = "Admin";
        // Thêm 2 dòng này vào dưới cùng của class PhieuThuChi và ThietLap
        [ForeignKey("CuaHangId")]
        public virtual CuaHang? CuaHang { get; set; }

        [ForeignKey("ChiNhanhId")] // (Chỉ thêm dòng này cho PhieuThuChi)
        public virtual ChiNhanh? ChiNhanh { get; set; }
    }
}