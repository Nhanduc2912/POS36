using System;
using System.Collections.Generic;

namespace POS36.Api.Models
{
    public class PhieuNhap
    {
        public int Id { get; set; }

        // --- 3 CỘT MỚI BỔ SUNG ĐỂ SỬA LỖI ---
        public int CuaHangId { get; set; }
        public string MaChungTu { get; set; } = string.Empty;
        public string TrangThai { get; set; } = "Đang xử lý";
        // -------------------------------------

        public int ChiNhanhId { get; set; }
        public int TaiKhoanId { get; set; } // Người tạo phiếu
        public DateTime NgayNhap { get; set; }
        public string GhiChu { get; set; } = string.Empty;

        public List<ChiTietPhieuNhap> ChiTiets { get; set; } = new();
    }
}