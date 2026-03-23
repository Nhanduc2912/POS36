using System;
using System.Collections.Generic;

namespace POS36.Api.Models
{
    public class PhieuKiemKe
    {
        public int Id { get; set; }
        public string MaChungTu { get; set; } = string.Empty;
        public int CuaHangId { get; set; }
        public int ChiNhanhId { get; set; }
        public DateTime NgayTao { get; set; }
        public string TrangThai { get; set; } = "Đang xử lý"; // Hoặc "Hoàn thành"
        public string GhiChu { get; set; } = string.Empty;

        // Navigation property
        public List<ChiTietKiemKe> ChiTiets { get; set; } = new();
    }
}