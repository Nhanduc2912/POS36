using System.ComponentModel.DataAnnotations;

namespace POS36.Api.Models
{
    public class NguyenVatLieu
    {
        [Key]
        public int Id { get; set; }
        
        public int CuaHangId { get; set; }
        
        // Có thể liên kết với DanhMucNguyenVatLieu nếu muốn phân loại nguyên vật liệu (Thịt, Rau củ, Gia vị...)
        public int? DanhMucNguyenVatLieuId { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string TenNguyenVatLieu { get; set; } = string.Empty;
        
        // Đơn vị tính cơ bản để định lượng (g, ml, cái, hộp...)
        public string DonViTinh { get; set; } = string.Empty;
        
        // Cảnh báo khi tồn kho tổng dưới mức này
        public decimal NguongCanhBao { get; set; } = 0;
        
        // Báo trước hạn sử dụng bao nhiêu ngày
        public int SoNgayCanhBaoHetHan { get; set; } = 5;
        
        public bool TrangThai { get; set; } = true;
        
        /// <summary>
        /// Giá vốn bình quân gia quyền (Moving Average Cost - MAC).
        /// Được cập nhật tự động mỗi khi có phiếu nhập hàng hoàn thành.
        /// Công thức: (Tồn cũ × Giá vốn cũ + SL nhập × Đơn giá nhập) / (Tồn cũ + SL nhập)
        /// </summary>
        public decimal GiaVonHienTai { get; set; } = 0;
        
        public DanhMucNguyenVatLieu? DanhMucNguyenVatLieu { get; set; }
    }
}
