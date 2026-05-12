using System.ComponentModel.DataAnnotations;

namespace POS36.Api.Models
{
    /// <summary>
    /// Định nghĩa các gói cho thuê phần mềm POS36.
    /// SuperAdmin có thể tùy chỉnh giá và giới hạn qua CRUD.
    /// </summary>
    public class GoiDichVu
    {
        [Key] public int Id { get; set; }

        public string TenGoi { get; set; } = string.Empty;     // "Gói Starter", "Gói Pro", "Gói Ultimate"
        public string MaGoi { get; set; } = string.Empty;      // "STARTER", "PRO", "ULTIMATE"
        public int SoThang { get; set; }                        // 6, 12, 24
        public decimal GiaThang { get; set; }                   // Giá quy đổi mỗi tháng (VND)
        public decimal TongGia { get; set; }                    // Tổng giá = GiaThang × SoThang

        // Giới hạn (0 = không giới hạn)
        public int GioiHanHoaDon { get; set; } = 0;            // Số hóa đơn tối đa mỗi tháng
        public int GioiHanNhanVien { get; set; } = 0;          // Số nhân viên tối đa

        public string? MoTa { get; set; }
        public bool IsActive { get; set; } = true;
        public int ThuTuHienThi { get; set; } = 0;             // Thứ tự trên trang Pricing
    }
}
