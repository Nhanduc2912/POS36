using System;
using System.Collections.Generic;

namespace POS36.Api.Models
{

    public class ChiTietKiemKe
    {
        public int Id { get; set; }
        public int PhieuKiemKeId { get; set; }
        public int NguyenVatLieuId { get; set; }
        public DateTime? NgayHetHan { get; set; } // Lô hàng nào

        public decimal TonKhoHienTai { get; set; } // Tồn kho hệ thống của Lô này
        public decimal SoLuongKiemKe { get; set; } // Số lượng đếm thực tế

        // Navigation properties
        public PhieuKiemKe? PhieuKiemKe { get; set; }
        public NguyenVatLieu? NguyenVatLieu { get; set; }
    }
}