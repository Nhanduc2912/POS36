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

                // FIX-3: Dùng aggregate query trực tiếp trên DB — không .ToListAsync() toàn bảng
                var hoaDonQuery = _context.HoaDons
                    .Where(h => h.CuaHangId == cuaHangId && h.ChiNhanhId == chiNhanhId);

                // 1. TỔNG HỢP — tính thẳng trên DB
                int tongDonHang = await hoaDonQuery.CountAsync(h => h.TrangThai == "Đã thanh toán");
                decimal doanhThu = await hoaDonQuery
                    .Where(h => h.TrangThai == "Đã thanh toán")
                    .SumAsync(h => (decimal?)h.TongTien) ?? 0;
                int donHuy = await hoaDonQuery
                    .CountAsync(h => h.TrangThai.ToLower().Contains("hủy"));

                // 2. TẠM TÍNH HIỆN TẠI
                decimal tamTinh = await hoaDonQuery
                    .Where(h => h.TrangThai == "Đang phục vụ")
                    .SumAsync(h => (decimal?)h.TongTien) ?? 0;

                // 3. DOANH THU THEO PHƯƠNG THỨC — tính trên DB
                var phieuQuery = _context.PhieuThuChis
                    .Where(p => p.CuaHangId == cuaHangId
                             && p.ChiNhanhId == chiNhanhId
                             && p.LoaiPhieu == "Thu"
                             && p.HangMuc == "Thu tiền bán hàng");

                double tienMat = await phieuQuery
                    .Where(p => p.PhuongThuc == "Tiền mặt")
                    .SumAsync(p => (double?)p.GiaTri) ?? 0;
                double chuyenKhoan = await phieuQuery
                    .Where(p => p.PhuongThuc == "Chuyển khoản")
                    .SumAsync(p => (double?)p.GiaTri) ?? 0;

                // 4. BÀN — giữ nguyên (cần danh sách để đếm trạng thái)
                var bans = await _context.Bans
                    .Include(b => b.KhuVuc)
                    .Where(b => b.CuaHangId == cuaHangId
                             && (b.KhuVuc == null || b.KhuVuc.ChiNhanhId == chiNhanhId))
                    .ToListAsync();

                int tongBan = bans.Count;
                int banDangDung = bans.Count(b => b.TrangThai == "Đang phục vụ");

                // 5. CẢNH BÁO TỒN KHO — dùng NgưỡngCanhBao từng sản phẩm (FEAT-2)
                int canhBaoKho = await _context.TonKhos
                    .Where(t => t.ChiNhanhId == chiNhanhId)
                    .Join(_context.SanPhams,
                        t => t.SanPhamId,
                        s => s.Id,
                        (t, s) => new { t.SoLuong, s.NgưỡngCanhBao })
                    .CountAsync(x => x.SoLuong <= x.NgưỡngCanhBao);

                // 6. BIỂU ĐỒ 7 NGÀY — select tối thiểu, không cả bảng
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
                        tongDonHang,
                        doanhThu,
                        tamTinh,
                        tienMat,
                        chuyenKhoan,
                        donHuy,
                        canhBaoKho,
                        banDangDung,
                        tongBan
                    },
                    chart = new { labels, data = chartData }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi khi lấy dữ liệu Dashboard: " + ex.Message);
            }
        }
    }
}