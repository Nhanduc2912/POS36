using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS36.Api.Models
{
    public class NhatKyHoatDong
    {
        [Key]
        public int Id { get; set; }

        public int CuaHangId { get; set; }
        public int ChiNhanhId { get; set; }

        [Required, MaxLength(100)]
        public string NguoiThucHien { get; set; } = string.Empty; // Username hoặc tên nhân viên

        [Required, MaxLength(50)]
        public string VaiTro { get; set; } = string.Empty; // Vai trò: ChuCuaHang, ThuNgan, Order, Bep, Admin, QuanLy...

        [Required, MaxLength(100)]
        public string HanhDong { get; set; } = string.Empty; // Tên hành động: "GoiMon", "ThanhToan", "HuyMon", "NhapKho", "CapNhatBan", "TachBan", "GhepBan"

        [Required, MaxLength(1000)]
        public string MoTa { get; set; } = string.Empty; // Nội dung mô tả chi tiết: "Nguyễn Văn A đã thanh toán hóa đơn bàn 1. Tổng tiền: 150.000đ"

        public DateTime ThoiGian { get; set; } = DateTime.Now; // Ngày giờ thực hiện chi tiết đến giây

        [MaxLength(50)]
        public string? IpAddress { get; set; } // Địa chỉ IP người thực hiện

        [MaxLength(200)]
        public string? ThietBi { get; set; } // Thiết bị / User agent

        [ForeignKey("CuaHangId")]
        public virtual CuaHang? CuaHang { get; set; }

        [ForeignKey("ChiNhanhId")]
        public virtual ChiNhanh? ChiNhanh { get; set; }
    }
}
