using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace POS36.Api.DTOs
{
    public class TaoPhieuKiemKeDto
    {
        public int ChiNhanhId { get; set; }
        public string GhiChu { get; set; } = string.Empty;

        // Trạng thái: "Đang xử lý" (Chỉ lưu nháp) hoặc "Hoàn thành" (Chốt sổ, cân bằng kho)
        public string TrangThai { get; set; } = "Đang xử lý";

        [Required]
        public List<ChiTietKiemKeDto> ChiTiets { get; set; } = new();
    }

    public class ChiTietKiemKeDto
    {
        public int SanPhamId { get; set; }
        public int TonKhoHienTai { get; set; } // Tồn kho trên phần mềm lúc bắt đầu đếm
        public int SoLuongKiemKe { get; set; } // Số lượng thực tế nhân viên đếm được
    }
}