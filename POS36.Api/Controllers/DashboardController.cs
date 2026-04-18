using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCuaHangId()
        {
            var claim = User.FindFirst("CuaHangId");
            if (claim == null) throw new UnauthorizedAccessException("Token không hợp lệ");
            return int.Parse(claim.Value);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] int chiNhanhId)
        {
            try
            {
                int cuaHangId = GetCuaHangId();
                DateTime sevenDaysAgo = DateTime.Today.AddDays(-6);

                // 1. TỔNG HỢP HÓA ĐƠN TOÀN THỜI GIAN (Gỡ bỏ điều kiện ngày)
                var hoaDonsToanThoiGian = await _context.HoaDons
                    .Where(h => h.CuaHangId == cuaHangId && h.ChiNhanhId == chiNhanhId)
                    .ToListAsync();

                int tongDonHang = hoaDonsToanThoiGian.Count(h => h.TrangThai == "Đã thanh toán");
                decimal doanhThu = hoaDonsToanThoiGian.Where(h => h.TrangThai == "Đã thanh toán").Sum(h => h.TongTien);

                // Đếm các đơn bị hủy (Bao gồm cả "Đã hủy" hoặc "Đã Hủy" do em gõ khác case)
                int donHuy = hoaDonsToanThoiGian.Count(h => h.TrangThai.ToLower().Contains("hủy"));

                // 2. TẠM TÍNH HIỆN TẠI (Các bàn đang ngồi)
                var tamTinh = await _context.HoaDons
                    .Where(h => h.CuaHangId == cuaHangId && h.ChiNhanhId == chiNhanhId && h.TrangThai == "Đang phục vụ")
                    .SumAsync(h => (decimal?)h.TongTien) ?? 0;

                // 3. DOANH THU THEO PHƯƠNG THỨC THANH TOÁN (Toàn thời gian)
                var phieuThusToanThoiGian = await _context.PhieuThuChis
                    .Where(p => p.CuaHangId == cuaHangId && p.ChiNhanhId == chiNhanhId && p.LoaiPhieu == "Thu" && p.HangMuc == "Thu tiền bán hàng")
                    .ToListAsync();

                double tienMat = phieuThusToanThoiGian.Where(p => p.PhuongThuc == "Tiền mặt").Sum(p => p.GiaTri);
                double chuyenKhoan = phieuThusToanThoiGian.Where(p => p.PhuongThuc == "Chuyển khoản").Sum(p => p.GiaTri);

                var bans = await _context.Bans
                    .Include(b => b.KhuVuc)
                    .Where(b => b.CuaHangId == cuaHangId && (b.KhuVuc == null || b.KhuVuc.ChiNhanhId == chiNhanhId))
                    .ToListAsync();

                int tongBan = bans.Count;
                int banDangDung = bans.Count(b => b.TrangThai == "Đang phục vụ");

                // 5. CẢNH BÁO TỒN KHO
                int canhBaoKho = await _context.TonKhos
                    .Where(t => t.ChiNhanhId == chiNhanhId && t.SoLuong <= 5)
                    .CountAsync();

                // 6. BIỂU ĐỒ DOANH THU 7 NGÀY QUA (Vẫn giữ 7 ngày để vẽ biểu đồ cho đẹp)
                // BUG #4 FIX: Dùng NgayThanhToan thay NgayTao để tính đúng ngày thanh toán
                var recentOrders = await _context.HoaDons
                    .Where(h => h.CuaHangId == cuaHangId && h.ChiNhanhId == chiNhanhId
                             && h.TrangThai == "Đã thanh toán"
                             && h.NgayThanhToan >= sevenDaysAgo)
                    .Select(h => new { h.NgayThanhToan, h.TongTien })
                    .ToListAsync();

                var labels = new List<string>();
                var chartData = new List<decimal>();

                for (int i = 0; i <= 6; i++)
                {
                    DateTime date = sevenDaysAgo.AddDays(i);
                    labels.Add(date.ToString("dd/MM"));

                    decimal dailyTotal = recentOrders
                        .Where(o => o.NgayThanhToan.HasValue && o.NgayThanhToan.Value.Date == date)
                        .Sum(o => o.TongTien);

                    chartData.Add(dailyTotal);
                }

                return Ok(new
                {
                    summary = new
                    {
                        tongDonHang = tongDonHang,
                        doanhThu = doanhThu,
                        tamTinh = tamTinh,
                        tienMat = tienMat,
                        chuyenKhoan = chuyenKhoan,
                        donHuy = donHuy,
                        canhBaoKho = canhBaoKho,
                        banDangDung = banDangDung,
                        tongBan = tongBan
                    },
                    chart = new { labels = labels, data = chartData }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi khi lấy dữ liệu Dashboard: " + ex.Message);
            }
        }
    }
}