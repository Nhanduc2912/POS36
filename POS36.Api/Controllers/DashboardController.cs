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

                var branchClaim = User.FindFirst("ChiNhanhId");
                if (branchClaim != null)
                {
                    int userBranchId = int.Parse(branchClaim.Value);
                    if (chiNhanhId > 0 && chiNhanhId != userBranchId)
                    {
                        return StatusCode(403, "Bạn không có quyền truy cập dữ liệu của chi nhánh khác!");
                    }
                    chiNhanhId = userBranchId;
                }
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

                // 5. CẢNH BÁO TỒN KHO (Gộp Sắp hết hàng & Sắp hết hạn)
                var now = DateTime.Now;
                var allTonKho = await _context.TonKhos
                    .Include(t => t.NguyenVatLieu)
                    .Where(t => t.ChiNhanhId == chiNhanhId && t.NguyenVatLieu != null)
                    .ToListAsync();

                // 5.1 Đếm Nguyên vật liệu sắp hết hàng (Tính theo Tổng Tồn Kho <= Ngưỡng)
                int canhBaoHetHang = allTonKho
                    .GroupBy(t => t.NguyenVatLieuId)
                    .Count(g => g.Sum(t => t.SoLuong) <= g.First().NguyenVatLieu!.NguongCanhBao);

                // 5.2 Đếm Lô sắp hết hạn
                int canhBaoHetHan = allTonKho
                    .Count(t => t.SoLuong > 0 && t.NgayHetHan.HasValue 
                                && (t.NgayHetHan.Value - now).TotalDays <= t.NguyenVatLieu!.SoNgayCanhBaoHetHan);

                int canhBaoKho = canhBaoHetHang + canhBaoHetHan;

                // 6. BIỂU ĐỒ 7 NGÀY — select tối thiểu, không cả bảng
                var recentOrders = await _context.HoaDons
                    .Where(h => h.CuaHangId == cuaHangId && h.ChiNhanhId == chiNhanhId
                             && h.TrangThai == "Đã thanh toán"
                             && h.NgayThanhToan >= sevenDaysAgo)
                    .Select(h => new { h.NgayThanhToan, h.TongTien })
                    .ToListAsync();

                var labels = new List<string>();
                var doanhThuData = new List<decimal>();
                var donHangData = new List<int>();

                for (int i = 0; i <= 6; i++)
                {
                    DateTime date = sevenDaysAgo.AddDays(i);
                    labels.Add(date.ToString("dd/MM"));
                    var ordersOnDate = recentOrders.Where(o => o.NgayThanhToan.HasValue && o.NgayThanhToan.Value.Date == date).ToList();
                    doanhThuData.Add(ordersOnDate.Sum(o => o.TongTien));
                    donHangData.Add(ordersOnDate.Count);
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
                    chart = new { labels, doanhThu = doanhThuData, donHang = donHangData }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi khi lấy dữ liệu Dashboard: " + ex.Message);
            }
        }

        [HttpGet("canhbao-chitiet")]
        public async Task<IActionResult> GetCanhBaoChiTiet([FromQuery] int chiNhanhId)
        {
            try
            {
                var branchClaim = User.FindFirst("ChiNhanhId");
                if (branchClaim != null)
                {
                    int userBranchId = int.Parse(branchClaim.Value);
                    if (chiNhanhId > 0 && chiNhanhId != userBranchId)
                        return StatusCode(403, "Bạn không có quyền truy cập dữ liệu của chi nhánh khác!");
                    chiNhanhId = userBranchId;
                }

                var now = DateTime.Now;
                var allTonKho = await _context.TonKhos
                    .Include(t => t.NguyenVatLieu)
                    .Where(t => t.ChiNhanhId == chiNhanhId && t.NguyenVatLieu != null)
                    .ToListAsync();

                // 1. Sắp hết hàng
                var sapHetHang = allTonKho
                    .GroupBy(t => t.NguyenVatLieuId)
                    .Select(g => new { 
                        NguyenVatLieu = g.First().NguyenVatLieu, 
                        TongTon = g.Sum(t => t.SoLuong) 
                    })
                    .Where(x => x.TongTon <= x.NguyenVatLieu!.NguongCanhBao)
                    .Select(x => new
                    {
                        x.NguyenVatLieu!.Id,
                        x.NguyenVatLieu.TenNguyenVatLieu,
                        SoLuong = x.TongTon,
                        NguongCanhBao = x.NguyenVatLieu.NguongCanhBao,
                        x.NguyenVatLieu.DonViTinh
                    })
                    .ToList();

                // 2. Sắp hết hạn
                var sapHetHan = allTonKho
                    .Where(t => t.SoLuong > 0 && t.NgayHetHan.HasValue && (t.NgayHetHan.Value - now).TotalDays <= t.NguyenVatLieu!.SoNgayCanhBaoHetHan)
                    .Select(t => new
                    {
                        TonKhoId = t.Id,
                        t.NguyenVatLieu!.TenNguyenVatLieu,
                        t.SoLuong,
                        t.NguyenVatLieu.DonViTinh,
                        t.NgayHetHan,
                        SoNgayConLai = Math.Round((t.NgayHetHan!.Value - now).TotalDays, 0)
                    })
                    .OrderBy(x => x.SoNgayConLai)
                    .ToList();

                return Ok(new { SapHetHang = sapHetHang, SapHetHan = sapHetHan });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi: " + ex.Message);
            }
        }
    }
}