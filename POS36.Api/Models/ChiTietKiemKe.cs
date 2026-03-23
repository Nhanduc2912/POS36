using System;
using System.Collections.Generic;

namespace POS36.Api.Models
{

    public class ChiTietKiemKe
    {
        public int Id { get; set; }
        public int PhieuKiemKeId { get; set; }
        public int SanPhamId { get; set; }
        public int TonKhoHienTai { get; set; } // Tồn kho hệ thống
        public int SoLuongKiemKe { get; set; } // Số lượng đếm thực tế

        // Navigation properties
        public PhieuKiemKe? PhieuKiemKe { get; set; }
        public SanPham? SanPham { get; set; }
    }
}