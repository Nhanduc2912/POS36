using System.ComponentModel.DataAnnotations;

namespace POS36.Api.Models
{
    public class PhieuNhap
    {
        [Key]
        public int Id { get; set; }
        public int ChiNhanhId { get; set; }
        public int TaiKhoanId { get; set; } // Ai là người nhập

        public DateTime NgayNhap { get; set; } = DateTime.Now;
        public string GhiChu { get; set; } = string.Empty;

        // Tạm thời để đơn giản, anh gộp Chi tiết phiếu nhập dạng JSON hoặc xử lý mảng trực tiếp sau ở API
        // Nếu em muốn chi tiết từng dòng nhập, có thể tạo thêm bảng ChiTietPhieuNhap.
    }
}