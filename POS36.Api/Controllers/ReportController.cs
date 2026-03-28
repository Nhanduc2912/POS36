using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCuaHangId()
        {
            var claim = User.FindFirst("CuaHangId");
            if (claim == null) throw new UnauthorizedAccessException("Token không hợp lệ");
            return int.Parse(claim.Value);
        }

        // 1. API TỔNG KẾT CUỐI NGÀY (Dữ liệu thật)
        [HttpGet("daily")]
        public async Task<IActionResult> GetDailySummary([FromQuery] string date)
        {
            try
            {
                int cuaHangId = GetCuaHangId();
                DateTime targetDate = DateTime.TryParse(date, out var d) ? d.Date : DateTime.Now.Date;
                DateTime nextDay = targetDate.AddDays(1);

                var hoaDons = await _context.HoaDons
                    .Include(h => h.Ban)
                    .Where(h => h.CuaHangId == cuaHangId && h.TrangThai == "Đã thanh toán" && h.NgayTao >= targetDate && h.NgayTao < nextDay)
                    .ToListAsync();

                var result = new
                {
                    tongDoanhThu = hoaDons.Sum(h => h.TongTien),
                    tienMat = hoaDons.Where(h => h.PhuongThucThanhToan == "Tiền mặt").Sum(h => h.TongTien),
                    chuyenKhoan = hoaDons.Where(h => h.PhuongThucThanhToan == "Chuyển khoản").Sum(h => h.TongTien),
                    tongDon = hoaDons.Count,
                    danhSachDon = hoaDons.OrderByDescending(h => h.NgayTao).Select(h => new
                    {
                        id = h.Id,
                        maChungTu = $"HD{h.NgayTao:ddMMyy}-{h.Id:D4}",
                        ngayBan = h.NgayTao,
                        tenBan = h.Ban?.TenBan ?? "Mang về",
                        phuongThuc = h.PhuongThucThanhToan,
                        tongCong = h.TongTien
                    }).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // 2. API BÁO CÁO BÁN HÀNG (Dữ liệu thật)
        [HttpGet("sales")]
        public async Task<IActionResult> GetSalesReport([FromQuery] string fromDate, [FromQuery] string toDate)
        {
            try
            {
                int cuaHangId = GetCuaHangId();
                DateTime start = DateTime.TryParse(fromDate, out var s) ? s.Date : DateTime.Now.Date.AddDays(-30);
                DateTime end = DateTime.TryParse(toDate, out var e) ? e.Date.AddDays(1).AddTicks(-1) : DateTime.Now.Date.AddDays(1).AddTicks(-1);

                var topItems = await _context.ChiTietHoaDons
                    .Include(ct => ct.HoaDon)
                    .Include(ct => ct.SanPham)
                    .Where(ct => ct.HoaDon!.CuaHangId == cuaHangId && ct.HoaDon.TrangThai == "Đã thanh toán" && ct.HoaDon.NgayTao >= start && ct.HoaDon.NgayTao <= end)
                    .GroupBy(ct => ct.SanPham!.TenSanPham)
                    .Select(g => new
                    {
                        tenMon = g.Key ?? "Sản phẩm khác",
                        soLuong = g.Sum(x => x.SoLuong),
                        doanhThu = g.Sum(x => x.SoLuong * x.DonGia)
                    })
                    .OrderByDescending(x => x.soLuong)
                    .ToListAsync();

                return Ok(topItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}