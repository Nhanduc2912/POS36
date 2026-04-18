using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using POS36.Api.DTOs;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BaoCaoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BaoCaoController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCuaHangId()
        {
            var claim = User.FindFirst("CuaHangId");
            if (claim == null) throw new UnauthorizedAccessException("Token không hợp lệ");
            return int.Parse(claim.Value);
        }

        // ==========================================
        // LẤY BÁO CÁO TỔNG QUAN (Theo ngày và Chi nhánh)
        // ==========================================
        [HttpGet("tongquan")]
        public async Task<IActionResult> GetBaoCaoTongQuan(
            [FromQuery] int chiNhanhId,
            [FromQuery] DateTime? tuNgay,
            [FromQuery] DateTime? denNgay)
        {
            int cuaHangId = GetCuaHangId();

            // 1. Xử lý thời gian (Nếu không truyền lên thì mặc định lấy ngày hôm nay)
            DateTime start = tuNgay ?? DateTime.Today;
            DateTime end = denNgay ?? DateTime.Today.AddDays(1).AddTicks(-1); // Lấy đến 23:59:59 của ngày kết thúc

            // 2. Lấy danh sách Hóa đơn ĐÃ THANH TOÁN trong khoảng thời gian
            var queryHoaDon = _context.HoaDons
                .Where(h => h.CuaHangId == cuaHangId
                         && h.ChiNhanhId == chiNhanhId
                         && h.TrangThai == "Đã thanh toán"
                         && h.NgayThanhToan >= start
                         && h.NgayThanhToan <= end);

            // Tính Tổng tiền và Tổng đơn
            decimal tongDoanhThu = await queryHoaDon.SumAsync(h => h.TongTien);
            int tongSoDon = await queryHoaDon.CountAsync();

            // 3. Thống kê theo Phương thức thanh toán
            // BUG #3 FIX: Dữ liệu thanh toán lưu ở PhieuThuChis (không phải ThanhToans)
            var phieuThuTrongKy = await _context.PhieuThuChis
                .Where(p => p.CuaHangId == cuaHangId
                         && p.ChiNhanhId == chiNhanhId
                         && p.LoaiPhieu == "Thu"
                         && p.HangMuc == "Thu tiền bán hàng"
                         && p.NgayGiaoDich >= start
                         && p.NgayGiaoDich <= end)
                .GroupBy(p => p.PhuongThuc)
                .Select(g => new ThongKePhuongThucDto
                {
                    PhuongThuc = g.Key,
                    SoTien = (decimal)g.Sum(p => p.GiaTri)
                }).ToListAsync();

            // 4. Tìm Top 5 Món Bán Chạy Nhất
            var topMonBanChay = await _context.ChiTietHoaDons
                .Where(ct => ct.HoaDon != null
                          && ct.HoaDon.CuaHangId == cuaHangId
                          && ct.HoaDon.ChiNhanhId == chiNhanhId
                          && ct.HoaDon.TrangThai == "Đã thanh toán"
                          && ct.HoaDon.NgayThanhToan >= start
                          && ct.HoaDon.NgayThanhToan <= end)
                .GroupBy(ct => new { ct.SanPhamId, ct.SanPham!.TenSanPham })
                .Select(g => new MonBanChayDto
                {
                    TenSanPham = g.Key.TenSanPham,
                    SoLuongDaBan = g.Sum(ct => ct.SoLuong)
                })
                .OrderByDescending(m => m.SoLuongDaBan)
                .Take(5)
                .ToListAsync();

            // 5. Đóng gói kết quả trả về
            var result = new BaoCaoTongQuanDto
            {
                TongDoanhThu = tongDoanhThu,
                TongSoDonHang = tongSoDon,
                DoanhThuTheoPhuongThuc = phieuThuTrongKy, // BUG #3 FIX: đổi biến đúng
                TopMonBanChay = topMonBanChay
            };

            return Ok(result);
        }
    }
}