using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using POS36.Api.Data;
using POS36.Api.Hubs;
using POS36.Api.Models;
using Serilog;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<KitchenHub> _hubContext;
        private readonly IHttpContextAccessor _httpContext;

        public SuperAdminController(AppDbContext context, IHubContext<KitchenHub> hubContext, IHttpContextAccessor httpContext)
        {
            _context = context;
            _hubContext = hubContext;
            _httpContext = httpContext;
        }

        // Helper: lấy IP client
        private string? GetClientIp() =>
            _httpContext.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        // Helper: ghi nhật ký vào DB
        private async Task GhiLog(string hanhDong, string moTa, string? url = null, string? chiTiet = null)
        {
            _context.NhatKyHeThangs.Add(new NhatKyHeThong
            {
                HanhDong = hanhDong,
                MoTa = moTa,
                UrlLienQuan = url,
                IpAddress = GetClientIp(),
                NguoiThucHien = User.Identity?.Name ?? User.FindFirst("TenDangNhap")?.Value ?? "SuperAdmin",
                ThoiGian = DateTime.Now,
                ChiTietJson = chiTiet,
            });
            await _context.SaveChangesAsync();
        }

        // ==========================================
        // 1. DASHBOARD TỔNG QUAN
        // ==========================================
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var now = DateTime.Now;
            var dauThang = new DateTime(now.Year, now.Month, 1);

            int tongCuaHang = await _context.CuaHangs.CountAsync();
            int dangHoatDong = await _context.CuaHangs.CountAsync(c => c.TrangThai == "HoatDong");
            int dangDungThu = await _context.CuaHangs.CountAsync(c => c.TrangThai == "DungThu");
            int daHetHan = await _context.CuaHangs.CountAsync(c => c.TrangThai == "ChiDoc");
            int biKhoa = await _context.CuaHangs.CountAsync(c => c.TrangThai == "BiKhoa");
            int sapHetHan = await _context.CuaHangs
                .CountAsync(c => c.TrangThai == "HoatDong" && c.NgayHetHan <= now.AddDays(7));

            decimal doanhThuThang = await _context.LichSuDangKys
                .Where(l => l.TrangThai == "DaThanhToan" && l.NgayThanhToan >= dauThang)
                .SumAsync(l => (decimal?)l.SoTienThanhToan) ?? 0;

            decimal doanhThuTong = await _context.LichSuDangKys
                .Where(l => l.TrangThai == "DaThanhToan")
                .SumAsync(l => (decimal?)l.SoTienThanhToan) ?? 0;

            int luotTruyCapHomNay = await _context.LuotTruyCaps
                .CountAsync(l => l.ThoiGian >= DateTime.Today);

            // Lượt đăng nhập thực tế từ nhật ký hệ thống
            int luotDangNhap = await _context.NhatKyHeThangs
                .CountAsync(n => n.ThoiGian >= DateTime.Today && n.HanhDong == "DangNhap");

            int donChoXuLy = await _context.LichSuDangKys
                .CountAsync(l => l.TrangThai == "ChoThanhToan");

            // Doanh thu 12 tháng gần nhất
            var doanhThu12Thang = new List<object>();
            for (int i = 11; i >= 0; i--)
            {
                var thang = now.AddMonths(-i);
                var dau = new DateTime(thang.Year, thang.Month, 1);
                var cuoi = dau.AddMonths(1);
                decimal dt = await _context.LichSuDangKys
                    .Where(l => l.TrangThai == "DaThanhToan" && l.NgayThanhToan >= dau && l.NgayThanhToan < cuoi)
                    .SumAsync(l => (decimal?)l.SoTienThanhToan) ?? 0;
                doanhThu12Thang.Add(new { label = dau.ToString("MM/yyyy"), value = dt });
            }

            // Đăng ký mới 30 ngày gần nhất
            var dangKyMoi30Ngay = await _context.CuaHangs
                .Where(c => c.NgayDangKy >= now.AddDays(-30))
                .CountAsync();

            return Ok(new
            {
                tongCuaHang, dangHoatDong, dangDungThu, daHetHan, biKhoa, sapHetHan,
                doanhThuThang, doanhThuTong, luotTruyCapHomNay, luotDangNhap, donChoXuLy,
                dangKyMoi30Ngay,
                doanhThu12Thang
            });
        }

        // ==========================================
        // 2. DANH SÁCH CỬA HÀNG
        // ==========================================
        [HttpGet("stores")]
        public async Task<IActionResult> GetStores([FromQuery] string? trangThai, [FromQuery] string? search)
        {
            var query = _context.CuaHangs.AsQueryable();

            if (!string.IsNullOrEmpty(trangThai))
                query = query.Where(c => c.TrangThai == trangThai);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(c => c.TenCuaHang.Contains(search) || c.SoDienThoai.Contains(search));

            var stores = await query
                .OrderByDescending(c => c.NgayDangKy)
                .Select(c => new
                {
                    c.Id, c.TenCuaHang, c.SoDienThoai, c.Email,
                    c.TrangThai, c.GoiDichVu, c.NgayDangKy, c.NgayHetHan,
                    soNgayConLai = Math.Max(0, (c.NgayHetHan - DateTime.Now).Days),
                    soNhanVien = _context.NhanViens.Count(n => n.CuaHangId == c.Id),
                    soHoaDon = _context.HoaDons.Count(h => h.CuaHangId == c.Id)
                })
                .ToListAsync();

            return Ok(stores);
        }

        // ==========================================
        // 3. CHI TIẾT 1 CỬA HÀNG
        // ==========================================
        [HttpGet("stores/{id}")]
        public async Task<IActionResult> GetStoreDetail(int id)
        {
            var store = await _context.CuaHangs.FindAsync(id);
            if (store == null) return NotFound();

            int soNhanVien = await _context.NhanViens.CountAsync(n => n.CuaHangId == id);
            int soSanPham = await _context.SanPhams.CountAsync(s => s.CuaHangId == id);
            int soHoaDon = await _context.HoaDons.CountAsync(h => h.CuaHangId == id);
            decimal doanhThu = await _context.HoaDons
                .Where(h => h.CuaHangId == id && h.TrangThai == "Đã thanh toán")
                .SumAsync(h => (decimal?)h.TongTien) ?? 0;

            var lichSuGoi = await _context.LichSuDangKys
                .Include(l => l.GoiDichVu)
                .Where(l => l.CuaHangId == id)
                .OrderByDescending(l => l.NgayTao)
                .Select(l => new { l.Id, tenGoi = l.GoiDichVu!.TenGoi, l.TrangThai, l.SoTienThanhToan, l.NgayTao, l.NgayThanhToan })
                .ToListAsync();

            return Ok(new
            {
                store = new
                {
                    store.Id, store.TenCuaHang, store.SoDienThoai, store.Email,
                    store.DiaChi, store.TrangThai, store.GoiDichVu,
                    store.NgayDangKy, store.NgayHetHan, store.GhiChu
                },
                thongKe = new { soNhanVien, soSanPham, soHoaDon, doanhThu },
                lichSuGoi
            });
        }

        // ==========================================
        // 4. CẬP NHẬT THÔNG TIN CỬA HÀNG
        // ==========================================
        [HttpPut("stores/{id}/update")]
        public async Task<IActionResult> UpdateStore(int id, [FromBody] UpdateStoreAdminDto dto)
        {
            var store = await _context.CuaHangs.FindAsync(id);
            if (store == null) return NotFound();

            if (!string.IsNullOrEmpty(dto.TenCuaHang)) store.TenCuaHang = dto.TenCuaHang;
            if (!string.IsNullOrEmpty(dto.SoDienThoai)) store.SoDienThoai = dto.SoDienThoai;
            if (dto.Email != null) store.Email = dto.Email;
            if (dto.DiaChi != null) store.DiaChi = dto.DiaChi;
            if (dto.GhiChu != null) store.GhiChu = dto.GhiChu;

            await _context.SaveChangesAsync();
            Log.Information("🏪 SuperAdmin cập nhật cửa hàng [{Id}] {TenCuaHang}", id, store.TenCuaHang);
            return Ok(new { message = "Cập nhật thành công!" });
        }

        // ==========================================
        // 5. KHÓA / MỞ CỬA HÀNG
        // ==========================================
        [HttpPut("stores/{id}/toggle-status")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var store = await _context.CuaHangs.FindAsync(id);
            if (store == null) return NotFound();

            bool isKhoa;
            if (store.TrangThai == "BiKhoa")
            {
                store.TrangThai = "HoatDong";
                isKhoa = false;
                Log.Information("🔓 SuperAdmin MỞ KHÓA cửa hàng [{Id}] {TenCuaHang}", id, store.TenCuaHang);
            }
            else
            {
                store.TrangThai = "BiKhoa";
                isKhoa = true;
                Log.Warning("🔒 SuperAdmin KHÓA cửa hàng [{Id}] {TenCuaHang}", id, store.TenCuaHang);
            }

            await _context.SaveChangesAsync();

            if (isKhoa)
                await GhiLog("KhoaCuaHang", $"Khóa cửa hàng [{store.TenCuaHang}] (ID: {id})", "/super-admin/stores");
            else
                await GhiLog("MoKhoaCuaHang", $"Mở khóa cửa hàng [{store.TenCuaHang}] (ID: {id})", "/super-admin/stores");

            return Ok(new { message = $"Cửa hàng đã được chuyển sang trạng thái: {store.TrangThai}" });
        }

        // ==========================================
        // 6. GIA HẠN THỦ CÔNG
        // ==========================================
        [HttpPost("stores/{id}/extend")]
        public async Task<IActionResult> ExtendManual(int id, [FromBody] ExtendDto dto)
        {
            var store = await _context.CuaHangs.FindAsync(id);
            if (store == null) return NotFound();

            int soThangGiaHan = dto.SoThang;
            string tenGoi = store.GoiDichVu ?? "";

            // Nếu chọn gói mới → bắt buộc dùng số tháng của gói đó, không để tùy nhập
            if (!string.IsNullOrEmpty(dto.GoiDichVu))
            {
                var goiMoi = await _context.GoiDichVus.FirstOrDefaultAsync(g => g.MaGoi == dto.GoiDichVu);
                if (goiMoi == null)
                    return BadRequest(new { message = $"Không tìm thấy gói dịch vụ '{dto.GoiDichVu}'!" });

                soThangGiaHan = goiMoi.SoThang; // Lấy số tháng từ gói
                store.GoiDichVu = goiMoi.MaGoi;
                tenGoi = goiMoi.TenGoi;
            }

            var ngayBatDau = (store.NgayHetHan > DateTime.Now) ? store.NgayHetHan : DateTime.Now;
            store.NgayHetHan = ngayBatDau.AddMonths(soThangGiaHan);
            store.TrangThai = "HoatDong";

            await _context.SaveChangesAsync();
            Log.Information("⏱️ SuperAdmin gia hạn {SoThang} tháng cho [{Id}] {TenCuaHang} → Hết hạn: {NgayHetHan:dd/MM/yyyy}",
                soThangGiaHan, id, store.TenCuaHang, store.NgayHetHan);

            await GhiLog("GiaHanCuaHang",
                $"Gia hạn [{store.TenCuaHang}] gói [{tenGoi}] thêm {soThangGiaHan} tháng → hết hạn: {store.NgayHetHan:dd/MM/yyyy}",
                $"/super-admin/stores");

            return Ok(new { message = $"Đã gia hạn {soThangGiaHan} tháng (gói: {tenGoi}). Hạn mới: {store.NgayHetHan:dd/MM/yyyy}" });
        }

        // ==========================================
        // 7. QUẢN LÝ ĐƠN ĐĂNG KÝ
        // ==========================================
        [HttpGet("subscriptions")]
        public async Task<IActionResult> GetSubscriptions([FromQuery] string? trangThai)
        {
            var query = _context.LichSuDangKys
                .Include(l => l.CuaHang)
                .Include(l => l.GoiDichVu)
                .AsQueryable();

            if (!string.IsNullOrEmpty(trangThai))
                query = query.Where(l => l.TrangThai == trangThai);

            var result = await query
                .OrderByDescending(l => l.NgayTao)
                .Select(l => new
                {
                    l.Id, l.CuaHangId,
                    tenCuaHang = l.CuaHang != null ? l.CuaHang.TenCuaHang : "",
                    tenGoi = l.GoiDichVu != null ? l.GoiDichVu.TenGoi : "",
                    l.SoTienThanhToan, l.TrangThai, l.MaGiaoDich,
                    l.NgayTao, l.NgayThanhToan, l.NguoiDuyet
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpPut("subscriptions/{id}/approve")]
        public async Task<IActionResult> ApproveSubscription(int id)
        {
            var sub = await _context.LichSuDangKys.Include(l => l.GoiDichVu).FirstOrDefaultAsync(l => l.Id == id);
            if (sub == null) return NotFound();
            if (sub.TrangThai == "DaThanhToan") return BadRequest("Đơn này đã được duyệt rồi!");

            sub.TrangThai = "DaThanhToan";
            sub.NgayThanhToan = DateTime.Now;
            sub.NguoiDuyet = User.Identity?.Name ?? "SuperAdmin";

            // Gia hạn cửa hàng
            var store = await _context.CuaHangs.FindAsync(sub.CuaHangId);
            if (store != null && sub.GoiDichVu != null)
            {
                var ngayBatDau = (store.NgayHetHan > DateTime.Now) ? store.NgayHetHan : DateTime.Now;
                store.NgayHetHan = ngayBatDau.AddMonths(sub.GoiDichVu.SoThang);
                store.TrangThai = "HoatDong";
                store.GoiDichVu = sub.GoiDichVu.MaGoi;
            }

            await _context.SaveChangesAsync();
            Log.Information("✅ SuperAdmin duyệt đơn #{Id} cho cửa hàng [{CuaHangId}]", id, sub.CuaHangId);
            await GhiLog("DuyetDon",
                $"Duyệt đơn #{id} cửa hàng [{store?.TenCuaHang ?? sub.CuaHangId.ToString()}] gói [{sub.GoiDichVu?.TenGoi}]",
                $"/super-admin/subscriptions",
                $"{{\"subId\":{id},\"cuaHangId\":{sub.CuaHangId},\"tenGoi\":\"{sub.GoiDichVu?.TenGoi}\",\"ngayHetHan\":\"{store?.NgayHetHan:dd/MM/yyyy}\"}}");
            return Ok(new { message = "Duyệt đơn thành công! Cửa hàng đã được gia hạn." });
        }

        [HttpPut("subscriptions/{id}/reject")]
        public async Task<IActionResult> RejectSubscription(int id, [FromBody] RejectDto? dto)
        {
            var sub = await _context.LichSuDangKys.FindAsync(id);
            if (sub == null) return NotFound();

            sub.TrangThai = "DaHuy";
            sub.GhiChu = dto?.LyDo ?? "Từ chối bởi SuperAdmin";
            await _context.SaveChangesAsync();

            Log.Information("❌ SuperAdmin từ chối đơn #{Id}", id);
            await GhiLog("TuChoiDon",
                $"Từ chối đơn #{id}. Lý do: {sub.GhiChu}",
                $"/super-admin/subscriptions");
            return Ok(new { message = "Đã từ chối đơn đăng ký." });
        }

        // ==========================================
        // 8. THỐNG KÊ TRUY CẬP
        // ==========================================
        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalytics([FromQuery] int days = 30)
        {
            var fromDate = DateTime.Now.AddDays(-days);

            // Lượt xem trang Landing Page công khai (không phải người dùng đăng nhập)
            int luotLandingPage = await _context.LuotTruyCaps.CountAsync(l => l.ThoiGian >= fromDate);
            int mobile = await _context.LuotTruyCaps.CountAsync(l => l.ThoiGian >= fromDate && l.ThietBi == "Mobile");
            int desktop = luotLandingPage - mobile;

            // Lượt đăng nhập thực tế (hoạt động hệ thống)
            int luotDangNhap = await _context.NhatKyHeThangs
                .CountAsync(n => n.ThoiGian >= fromDate && n.HanhDong == "DangNhap");

            // Lượt truy cập Landing Page theo ngày
            var luotTheoNgay = await _context.LuotTruyCaps
                .Where(l => l.ThoiGian >= fromDate)
                .GroupBy(l => l.ThoiGian.Date)
                .Select(g => new { label = g.Key.ToString("dd/MM"), value = g.Count() })
                .OrderBy(x => x.label)
                .ToListAsync();

            // Top trang Landing Page được xem nhiều nhất
            var topPages = await _context.LuotTruyCaps
                .Where(l => l.ThoiGian >= fromDate)
                .GroupBy(l => l.Url)
                .Select(g => new { url = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .Take(10)
                .ToListAsync();

            // Giờ cao điểm dựa trên NhatKyHeThangs (hoạt động thực tế của người dùng)
            var rawHourly = await _context.NhatKyHeThangs
                .Where(n => n.ThoiGian >= fromDate)
                .GroupBy(n => n.ThoiGian.Hour)
                .Select(g => new { hour = g.Key, count = g.Count() })
                .ToListAsync();

            var hourlyUsage = Enumerable.Range(0, 24).Select(h =>
            {
                var found = rawHourly.FirstOrDefault(r => r.hour == h);
                return new { hour = h < 10 ? "0" + h : h.ToString(), count = found?.count ?? 0 };
            }).ToList<object>();

            // Feature usage từ nhật ký hệ thống
            var featureRaw = await _context.NhatKyHeThangs
                .Where(n => n.ThoiGian >= fromDate)
                .GroupBy(n => n.HanhDong)
                .Select(g => new { hanhDong = g.Key, count = g.Count() })
                .OrderByDescending(g => g.count)
                .ToListAsync();

            var maxFeature = featureRaw.Any() ? featureRaw.Max(f => f.count) : 1;
            var featureUsage = featureRaw.Select(f => new
            {
                name = f.hanhDong,
                count = f.count,
                pct = Math.Round((double)f.count / maxFeature * 100, 0)
            }).ToList<object>();

            // Distinct log action types để dropdown lọc trong UI
            var logActions = await _context.NhatKyHeThangs
                .Select(n => n.HanhDong)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();

            return Ok(new
            {
                tongLuot = luotLandingPage,   // Chú ý: chỉ là lượt xem Landing Page
                luotDangNhap,                  // Lượt đăng nhập thực tế
                mobile, desktop,
                luotTheoNgay, topPages,
                hourlyUsage, featureUsage,
                logActions                     // Dùng cho dropdown filter nhật ký
            });
        }

        // ==========================================
        // 8B. LẤY DISTINCT LOG ACTIONS (dùng cho filter dropdown)
        // ==========================================
        [HttpGet("log-actions")]
        public async Task<IActionResult> GetLogActions()
        {
            var actions = await _context.NhatKyHeThangs
                .Select(n => n.HanhDong)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();
            return Ok(actions);
        }

        // ==========================================
        // 9. CRUD GÓI DỊCH VỤ
        // ==========================================
        [HttpGet("plans")]
        public async Task<IActionResult> GetPlans()
        {
            var plans = await _context.GoiDichVus.OrderBy(g => g.ThuTuHienThi).ToListAsync();
            return Ok(plans);
        }

        [HttpPost("plans")]
        public async Task<IActionResult> CreatePlan([FromBody] Models.GoiDichVu plan)
        {
            _context.GoiDichVus.Add(plan);
            await _context.SaveChangesAsync();
            Log.Information("➕ SuperAdmin tạo gói dịch vụ [{MaGoi}] {TenGoi}", plan.MaGoi, plan.TenGoi);
            await GhiLog("TaoGoiDichVu",
                $"Tạo gói dịch vụ mới: [{plan.MaGoi}] {plan.TenGoi} — {plan.SoThang} tháng — {plan.TongGia:N0}đ",
                "/super-admin/plans",
                $"{{\"maGoi\":\"{plan.MaGoi}\",\"tenGoi\":\"{plan.TenGoi}\",\"soThang\":{plan.SoThang},\"tongGia\":{plan.TongGia}}}");
            return Ok(new { message = "Tạo gói dịch vụ thành công!", id = plan.Id });
        }

        [HttpPut("plans/{id}")]
        public async Task<IActionResult> UpdatePlan(int id, [FromBody] Models.GoiDichVu dto)
        {
            var plan = await _context.GoiDichVus.FindAsync(id);
            if (plan == null) return NotFound();

            plan.TenGoi = dto.TenGoi;
            plan.MaGoi = dto.MaGoi;
            plan.SoThang = dto.SoThang;
            plan.GiaThang = dto.GiaThang;
            plan.TongGia = dto.TongGia;
            plan.GioiHanHoaDon = dto.GioiHanHoaDon;
            plan.GioiHanNhanVien = dto.GioiHanNhanVien;
            plan.MoTa = dto.MoTa;
            plan.IsActive = dto.IsActive;
            plan.ThuTuHienThi = dto.ThuTuHienThi;

            await _context.SaveChangesAsync();
            Log.Information("✏️ SuperAdmin cập nhật gói [{MaGoi}] {TenGoi}", plan.MaGoi, plan.TenGoi);
            await GhiLog("SuaGoiDichVu",
                $"Cập nhật gói [{plan.MaGoi}] {plan.TenGoi} — {plan.SoThang} tháng — IsActive: {plan.IsActive}",
                "/super-admin/plans");
            return Ok(new { message = "Cập nhật gói thành công!" });
        }

        [HttpDelete("plans/{id}")]
        public async Task<IActionResult> DeletePlan(int id)
        {
            var plan = await _context.GoiDichVus.FindAsync(id);
            if (plan == null) return NotFound(new { message = "Không tìm thấy gói dịch vụ!" });

            // Kiểm tra xem có cửa hàng nào đang sử dụng gói này không
            bool dangDuoc = await _context.CuaHangs.AnyAsync(c => c.GoiDichVu == plan.MaGoi);
            if (dangDuoc)
                return BadRequest(new { message = $"Không thể xóa! Đang có cửa hàng sử dụng gói \"{plan.TenGoi}\". Hãy ẩn gói thay vì xóa." });

            _context.GoiDichVus.Remove(plan);
            await _context.SaveChangesAsync();
            Log.Information("🗑️ SuperAdmin xóa gói dịch vụ [{Id}] {TenGoi}", id, plan.TenGoi);
            await GhiLog("XoaGoiDichVu",
                $"Xóa gói dịch vụ: [{plan.MaGoi}] {plan.TenGoi}",
                "/super-admin/plans");
            return Ok(new { message = $"Đã xóa gói \"{plan.TenGoi}\" thành công!" });
        }

        // ==========================================
        // 10. GỬI THÔNG BÁO
        // ==========================================
        [HttpPost("notifications")]
        public async Task<IActionResult> SendNotification([FromBody] SendNotificationDto dto)
        {
            var thongBao = new Models.ThongBaoHeThong
            {
                CuaHangId = dto.CuaHangId, // null = gửi cho tất cả
                TieuDe = dto.TieuDe,
                NoiDung = dto.NoiDung,
                LoaiThongBao = dto.LoaiThongBao ?? "ThongTin"
            };

            _context.ThongBaoHeThongs.Add(thongBao);
            await _context.SaveChangesAsync();

            // Phát thông báo Real-time xuống các cửa hàng qua SignalR
            if (dto.CuaHangId.HasValue)
            {
                await _hubContext.Clients.Group($"store_{dto.CuaHangId.Value}")
                    .SendAsync("NhanThongBaoHeThong", thongBao.TieuDe, thongBao.NoiDung, thongBao.LoaiThongBao);
            }
            else
            {
                await _hubContext.Clients.All
                    .SendAsync("NhanThongBaoHeThong", thongBao.TieuDe, thongBao.NoiDung, thongBao.LoaiThongBao);
            }

            string target = dto.CuaHangId.HasValue ? $"cửa hàng [{dto.CuaHangId}]" : "TẤT CẢ cửa hàng";
            Log.Information("📧 SuperAdmin gửi thông báo [{LoaiThongBao}] đến {Target}: {TieuDe}",
                thongBao.LoaiThongBao, target, thongBao.TieuDe);
            await GhiLog("GuiThongBao",
                $"Gửi thông báo [{thongBao.LoaiThongBao}] đến {target}: {thongBao.TieuDe}",
                "/super-admin/notifications");

            return Ok(new { message = "Đã gửi thông báo thành công!" });
        }

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var list = await _context.ThongBaoHeThongs
                .OrderByDescending(t => t.NgayTao)
                .Take(50)
                .Select(t => new
                {
                    t.Id, t.TieuDe, t.NoiDung, t.LoaiThongBao, t.NgayTao, t.CuaHangId,
                    tenCuaHang = t.CuaHangId != null
                        ? _context.CuaHangs.Where(c => c.Id == t.CuaHangId).Select(c => c.TenCuaHang).FirstOrDefault()
                        : null
                })
                .ToListAsync();
            return Ok(list);
        }


        // ==========================================
        // 11. AI PHÂN TÍCH DASHBOARD (GEMINI)
        // ==========================================
        [HttpPost("ai-analyze")]
        public async Task<IActionResult> AiAnalyze([FromBody] AiAnalyzeDto dto, [FromServices] IConfiguration config)
        {
            string apiKey = config["GeminiAI:ApiKey"] ?? "";
            if (string.IsNullOrEmpty(apiKey) || apiKey.Contains("NHẬP"))
                return BadRequest("Chưa cấu hình GeminiAI:ApiKey trong appsettings!");

            string prompt = $@"Bạn là chuyên gia phân tích kinh doanh SaaS. Phân tích dữ liệu sau của nền tảng POS36 và đưa ra nhận xét, cảnh báo, đề xuất hành động cụ thể. 
Trả về HTML đẹp (dùng thẻ h3, ul, li, strong, span style). 
Dữ liệu:
- Tổng cửa hàng: {dto.TongCuaHang}
- Đang hoạt động: {dto.DangHoatDong}
- Đang dùng thử: {dto.DangDungThu}
- Bị khóa: {dto.BiKhoa}
- Doanh thu tháng: {dto.DoanhThuThang:N0} VNĐ
- Tổng doanh thu: {dto.DoanhThuTong:N0} VNĐ
- Sắp hết hạn (7 ngày): {dto.SapHetHan} cửa hàng
- Đơn chờ xử lý: {dto.DonChoXuLy}
Hãy nhận xét về: tỉ lệ chuyển đổi, cảnh báo doanh thu, đề xuất upsell, và các hành động ưu tiên.";

            var payload = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
            string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.1-flash-lite:generateContent?key={apiKey}";

            using var client = new System.Net.Http.HttpClient();
            var content = new System.Net.Http.StringContent(
                System.Text.Json.JsonSerializer.Serialize(payload),
                System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync(endpoint, content);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Log.Error("❌ Gemini AI analyze lỗi: {Error}", json);
                return StatusCode(500, "Lỗi AI: " + json);
            }

            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var aiText = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text").GetString() ?? "";
            aiText = aiText.Replace("```html", "").Replace("```", "").Trim();

            return Ok(new { htmlReport = aiText });
        }
    }

    // DTOs
    public class UpdateStoreAdminDto
    {
        public string? TenCuaHang { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string? DiaChi { get; set; }
        public string? GhiChu { get; set; }
    }

    public class ExtendDto
    {
        public int SoThang { get; set; }
        public string? GoiDichVu { get; set; }
    }

    public class RejectDto
    {
        public string? LyDo { get; set; }
    }

    public class SendNotificationDto
    {
        public int? CuaHangId { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public string? LoaiThongBao { get; set; }
    }

    public class AiAnalyzeDto
    {
        public int TongCuaHang { get; set; }
        public int DangHoatDong { get; set; }
        public int DangDungThu { get; set; }
        public int BiKhoa { get; set; }
        public decimal DoanhThuThang { get; set; }
        public decimal DoanhThuTong { get; set; }
        public int SapHetHan { get; set; }
        public int DonChoXuLy { get; set; }
    }
}
