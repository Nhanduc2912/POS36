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
        // 1. BÁO CÁO TỔNG QUAN (Theo ngày và Chi nhánh)
        // ==========================================
        [HttpGet("tongquan")]
        public async Task<IActionResult> GetBaoCaoTongQuan(
            [FromQuery] int chiNhanhId,
            [FromQuery] DateTime? tuNgay,
            [FromQuery] DateTime? denNgay)
        {
            int cuaHangId = GetCuaHangId();

            // 1. Xử lý thời gian
            DateTime start = tuNgay ?? DateTime.Today;
            DateTime end = denNgay ?? DateTime.Today.AddDays(1).AddTicks(-1);

            // 2. Hóa đơn đã thanh toán trong kỳ
            var queryHoaDon = _context.HoaDons
                .Where(h => h.CuaHangId == cuaHangId
                         && h.ChiNhanhId == chiNhanhId
                         && h.TrangThai == "Đã thanh toán"
                         && h.NgayThanhToan >= start
                         && h.NgayThanhToan <= end);

            decimal tongDoanhThu = await queryHoaDon.SumAsync(h => (decimal?)h.TongTien) ?? 0;
            int tongSoDon = await queryHoaDon.CountAsync();

            // 3. Thống kê theo Phương thức thanh toán
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

            // 4. Top 5 Món Bán Chạy Nhất
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

            var result = new BaoCaoTongQuanDto
            {
                TongDoanhThu = tongDoanhThu,
                TongSoDonHang = tongSoDon,
                DoanhThuTheoPhuongThuc = phieuThuTrongKy,
                TopMonBanChay = topMonBanChay
            };

            return Ok(result);
        }

        // ==========================================
        // 2. BÁO CÁO LÃI GỘP (FEAT-3)
        // Doanh thu - Giá vốn = Lãi gộp
        // GiaVon đã được lưu vào ChiTietHoaDon tại thời điểm gọi món (average cost)
        // ==========================================
        [HttpGet("lai-gop")]
        [Authorize(Roles = "SuperAdmin,ChuCuaHang,Admin,QuanLy")]
        public async Task<IActionResult> GetBaoCaoLaiGop(
            [FromQuery] int chiNhanhId,
            [FromQuery] DateTime? tuNgay,
            [FromQuery] DateTime? denNgay)
        {
            int cuaHangId = GetCuaHangId();

            DateTime start = tuNgay ?? DateTime.Today;
            DateTime end = denNgay ?? DateTime.Today.AddDays(1).AddTicks(-1);

            // Lấy tất cả chi tiết hóa đơn đã thanh toán trong kỳ
            var chiTiets = await _context.ChiTietHoaDons
                .Where(ct => ct.HoaDon != null
                          && ct.HoaDon.CuaHangId == cuaHangId
                          && ct.HoaDon.ChiNhanhId == chiNhanhId
                          && ct.HoaDon.TrangThai == "Đã thanh toán"
                          && ct.HoaDon.NgayThanhToan >= start
                          && ct.HoaDon.NgayThanhToan <= end)
                .Select(ct => new
                {
                    ct.SanPhamId,
                    TenSanPham = ct.SanPham != null ? ct.SanPham.TenSanPham : "SP đã xóa",
                    ct.SoLuong,
                    ct.DonGia,   // Giá bán thực tế
                    ct.GiaVon,   // Giá vốn tại thời điểm bán
                    NgayBan = ct.HoaDon!.NgayThanhToan
                })
                .ToListAsync();

            // Tính tổng lãi gộp
            decimal tongDoanhThu = chiTiets.Sum(ct => ct.SoLuong * ct.DonGia);
            decimal tongGiaVon = chiTiets.Sum(ct => ct.SoLuong * (decimal)ct.GiaVon);
            decimal tongLaiGop = tongDoanhThu - tongGiaVon;
            double tiLeLaiGop = tongDoanhThu > 0 ? (double)(tongLaiGop / tongDoanhThu * 100) : 0;

            // Lãi gộp theo từng sản phẩm
            var laiGopTheoSanPham = chiTiets
                .GroupBy(ct => new { ct.SanPhamId, ct.TenSanPham })
                .Select(g => new
                {
                    TenSanPham = g.Key.TenSanPham,
                    SoLuongBan = g.Sum(ct => ct.SoLuong),
                    DoanhThu = g.Sum(ct => ct.SoLuong * ct.DonGia),
                    GiaVon = g.Sum(ct => ct.SoLuong * (decimal)ct.GiaVon),
                    LaiGop = g.Sum(ct => ct.SoLuong * (ct.DonGia - (decimal)ct.GiaVon)),
                    TiLeLaiGop = g.Sum(ct => ct.SoLuong * ct.DonGia) > 0
                        ? Math.Round((double)(g.Sum(ct => ct.SoLuong * (ct.DonGia - (decimal)ct.GiaVon))
                                             / g.Sum(ct => ct.SoLuong * ct.DonGia) * 100), 1)
                        : 0.0
                })
                .OrderByDescending(x => x.LaiGop)
                .ToList();

            // Biểu đồ lãi gộp theo ngày (7 ngày gần nhất nếu không truyền kỳ)
            var laiGopTheoNgay = chiTiets
                .Where(ct => ct.NgayBan.HasValue)
                .GroupBy(ct => ct.NgayBan!.Value.Date)
                .Select(g => new
                {
                    Ngay = g.Key.ToString("dd/MM"),
                    DoanhThu = g.Sum(ct => ct.SoLuong * ct.DonGia),
                    GiaVon = g.Sum(ct => ct.SoLuong * (decimal)ct.GiaVon),
                    LaiGop = g.Sum(ct => ct.SoLuong * (ct.DonGia - (decimal)ct.GiaVon))
                })
                .OrderBy(x => x.Ngay)
                .ToList();

            return Ok(new
            {
                TongQuan = new
                {
                    TuNgay = start.ToString("dd/MM/yyyy"),
                    DenNgay = end.ToString("dd/MM/yyyy"),
                    TongDoanhThu = tongDoanhThu,
                    TongGiaVon = tongGiaVon,
                    TongLaiGop = tongLaiGop,
                    TiLeLaiGop = Math.Round(tiLeLaiGop, 1),
                    SanPhamChuaTinhVon = chiTiets.Count(ct => ct.GiaVon == 0),
                    LuuY = chiTiets.Any(ct => ct.GiaVon == 0)
                        ? "Một số sản phẩm chưa có phiếu nhập kho — giá vốn = 0, lãi gộp có thể bị phóng đại."
                        : null
                },
                TheoSanPham = laiGopTheoSanPham,
                TheoNgay = laiGopTheoNgay
            });
        }
    }
}