using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using Serilog;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SubscriptionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private const int EXPIRY_MINUTES = 10;
        private const int DAILY_LIMIT = 3;

        public SubscriptionController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCuaHangId()
        {
            var claim = User.FindFirst("CuaHangId");
            if (claim == null) throw new UnauthorizedAccessException("Token không hợp lệ");
            return int.Parse(claim.Value);
        }

        /// <summary>Lấy thông tin ngân hàng SuperAdmin (private helper)</summary>
        private async Task<(string bankCode, string bankAccountNo, string bankAccountName, bool configured)> GetSystemBankAsync()
        {
            var keys = new[] { "BankCode", "BankAccountNo", "BankAccountName" };
            var configs = await _context.CauHinhHeThangs
                .Where(c => c.NhomCauHinh == "Payment" && keys.Contains(c.MaKey))
                .ToListAsync();

            var dict = configs.ToDictionary(c => c.MaKey, c => c.GiaTri);
            var no = dict.GetValueOrDefault("BankAccountNo", "");
            return (
                dict.GetValueOrDefault("BankCode", ""),
                no,
                dict.GetValueOrDefault("BankAccountName", ""),
                !string.IsNullOrWhiteSpace(no)
            );
        }

        // ================================================================
        // 1. LẤY DANH SÁCH GÓI DỊCH VỤ
        // ================================================================
        [HttpGet("plans")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPlans()
        {
            var plans = await _context.GoiDichVus
                .Where(g => g.IsActive)
                .OrderBy(g => g.ThuTuHienThi)
                .Select(g => new
                {
                    g.Id, g.TenGoi, g.MaGoi, g.SoThang,
                    g.GiaThang, g.TongGia,
                    g.GioiHanHoaDon, g.GioiHanNhanVien,
                    g.MoTa
                })
                .ToListAsync();

            return Ok(plans);
        }

        // ================================================================
        // 2. XEM GÓI HIỆN TẠI
        // ================================================================
        [HttpGet("my-plan")]
        public async Task<IActionResult> GetMyPlan()
        {
            int cuaHangId = GetCuaHangId();
            var store = await _context.CuaHangs.FindAsync(cuaHangId);
            if (store == null) return NotFound();

            Models.GoiDichVu? currentPlan = null;
            if (!string.IsNullOrEmpty(store.GoiDichVu))
                currentPlan = await _context.GoiDichVus.FirstOrDefaultAsync(g => g.MaGoi == store.GoiDichVu);

            var dauThang = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            int hoaDonThangNay = await _context.HoaDons
                .Where(h => h.CuaHangId == cuaHangId && h.NgayTao >= dauThang)
                .CountAsync();

            return Ok(new
            {
                tenCuaHang = store.TenCuaHang,
                trangThai = store.TrangThai,
                ngayHetHan = store.NgayHetHan,
                ngayDangKy = store.NgayDangKy,
                goiDichVu = store.GoiDichVu,
                tenGoi = currentPlan?.TenGoi ?? "Dùng thử",
                gioiHanHoaDon = currentPlan?.GioiHanHoaDon ?? 0,
                gioiHanNhanVien = currentPlan?.GioiHanNhanVien ?? 0,
                hoaDonThangNay = hoaDonThangNay,
                soNgayConLai = Math.Max(0, (store.NgayHetHan - DateTime.Now).Days)
            });
        }

        // ================================================================
        // 3. TẠO ĐƠN MUA GÓI — Có giới hạn 10 phút & 3 lần/ngày
        // ================================================================
        [HttpPost("purchase")]
        public async Task<IActionResult> Purchase([FromBody] PurchaseDto dto)
        {
            int cuaHangId = GetCuaHangId();

            var goi = await _context.GoiDichVus.FindAsync(dto.GoiDichVuId);
            if (goi == null || !goi.IsActive)
                return BadRequest(new { errorCode = "INVALID_PLAN", message = "Gói dịch vụ không tồn tại hoặc đã ngưng!" });

            var today  = DateTime.Today;
            var tomRow = today.AddDays(1);

            // ---- 1. Tự động hủy các đơn quá 10 phút chưa thanh toán ----
            var hetHanTruoc = DateTime.Now.AddMinutes(-EXPIRY_MINUTES);
            var donHetHan = await _context.LichSuDangKys
                .Where(l => l.CuaHangId == cuaHangId
                         && l.TrangThai == "ChoThanhToan"
                         && l.NgayTao < hetHanTruoc)
                .ToListAsync();
            foreach (var d in donHetHan)
            {
                d.TrangThai = "DaHuy";
                d.GhiChu = $"Tự động hủy: quá {EXPIRY_MINUTES} phút không thanh toán";
            }
            if (donHetHan.Any()) await _context.SaveChangesAsync();

            // ---- 2. Kiểm tra đơn đang ChoThanhToan còn hiệu lực ----
            var donDangCho = await _context.LichSuDangKys
                .Include(l => l.GoiDichVu)
                .FirstOrDefaultAsync(l => l.CuaHangId == cuaHangId && l.TrangThai == "ChoThanhToan");

            if (donDangCho != null)
            {
                var giayConLai = (int)(donDangCho.NgayTao.AddMinutes(EXPIRY_MINUTES) - DateTime.Now).TotalSeconds;
                return BadRequest(new
                {
                    errorCode    = "PENDING_ORDER",
                    message      = "Bạn đang có đơn chờ thanh toán. Hoàn tất hoặc hủy trước!",
                    donDangKyId  = donDangCho.Id,
                    maGiaoDich   = donDangCho.MaGiaoDich,
                    tongTien     = donDangCho.SoTienThanhToan,
                    tenGoi       = donDangCho.GoiDichVu?.TenGoi ?? "",
                    giayConLai   = Math.Max(0, giayConLai)
                });
            }

            // ---- 3. Kiểm tra giới hạn 3 lần/ngày ----
            int soLanHomNay = await _context.LichSuDangKys
                .Where(l => l.CuaHangId == cuaHangId
                         && l.NgayTao >= today
                         && l.NgayTao < tomRow)
                .CountAsync();

            if (soLanHomNay >= DAILY_LIMIT)
                return BadRequest(new
                {
                    errorCode = "DAILY_LIMIT",
                    message   = $"Đã đạt giới hạn {DAILY_LIMIT} lần tạo mã/ngày. Vui lòng thử lại vào ngày mai!"
                });

            // ---- 4. Tạo đơn mới ----
            var store = await _context.CuaHangs.FindAsync(cuaHangId);
            DateTime ngayBatDau   = (store!.NgayHetHan > DateTime.Now) ? store.NgayHetHan : DateTime.Now;
            DateTime ngayKetThuc  = ngayBatDau.AddMonths(goi.SoThang);

            var lichSu = new Models.LichSuDangKy
            {
                CuaHangId           = cuaHangId,
                GoiDichVuId         = goi.Id,
                NgayBatDau          = ngayBatDau,
                NgayKetThuc         = ngayKetThuc,
                SoTienThanhToan     = goi.TongGia,
                TrangThai           = "ChoThanhToan",
                PhuongThucThanhToan = "SePay"
            };

            _context.LichSuDangKys.Add(lichSu);
            await _context.SaveChangesAsync();

            lichSu.MaGiaoDich = $"POS36G{lichSu.Id}";
            await _context.SaveChangesAsync();

            await _context.LogHoatDongAsync(
                int.Parse(User.FindFirst("ChiNhanhId")?.Value ?? "0"),
                "Nhật ký gói mua",
                $"Tạo yêu cầu thanh toán gói '{goi.TenGoi}' ({goi.SoThang}th) — {goi.TongGia:N0}đ — Mã: {lichSu.MaGiaoDich}. (Lần {soLanHomNay + 1}/{DAILY_LIMIT} hôm nay)");

            Log.Information("💳 CuaHang [{Id}] tạo đơn {GoiTen} — {Ma} ({Lan}/{Max} hôm nay)",
                cuaHangId, goi.TenGoi, lichSu.MaGiaoDich, soLanHomNay + 1, DAILY_LIMIT);

            return Ok(new
            {
                message       = "Đã tạo đơn thành công!",
                donDangKyId   = lichSu.Id,
                maGiaoDich    = lichSu.MaGiaoDich,
                tongTien      = goi.TongGia,
                tenGoi        = goi.TenGoi,
                soThang       = goi.SoThang,
                soLanConLai   = DAILY_LIMIT - (soLanHomNay + 1),
                hetHanLuc     = lichSu.NgayTao.AddMinutes(EXPIRY_MINUTES)
            });
        }

        // ================================================================
        // 4. LẤY CHI TIẾT CHECKOUT (gộp thông tin đơn + ngân hàng)
        // ================================================================
        [HttpGet("checkout-detail/{id}")]
        public async Task<IActionResult> GetCheckoutDetail(int id)
        {
            int cuaHangId = GetCuaHangId();

            var lichSu = await _context.LichSuDangKys
                .Include(l => l.GoiDichVu)
                .Include(l => l.CuaHang)
                .FirstOrDefaultAsync(l => l.Id == id && l.CuaHangId == cuaHangId);

            if (lichSu == null) return NotFound(new { message = "Không tìm thấy đơn hàng!" });

            // Tự động hủy nếu đã quá 10 phút
            if (lichSu.TrangThai == "ChoThanhToan" && lichSu.NgayTao.AddMinutes(EXPIRY_MINUTES) < DateTime.Now)
            {
                lichSu.TrangThai = "DaHuy";
                lichSu.GhiChu = $"Tự động hủy: quá {EXPIRY_MINUTES} phút không thanh toán";
                await _context.SaveChangesAsync();
            }

            var (bankCode, bankAccountNo, bankAccountName, configured) = await GetSystemBankAsync();

            var giayConLai = lichSu.TrangThai == "ChoThanhToan"
                ? (int)(lichSu.NgayTao.AddMinutes(EXPIRY_MINUTES) - DateTime.Now).TotalSeconds
                : 0;

            return Ok(new
            {
                donDangKyId     = lichSu.Id,
                maGiaoDich      = lichSu.MaGiaoDich,
                tenGoi          = lichSu.GoiDichVu?.TenGoi ?? "N/A",
                soTienThanhToan = lichSu.SoTienThanhToan,
                trangThai       = lichSu.TrangThai,
                ngayTao         = lichSu.NgayTao,
                hetHanLuc       = lichSu.NgayTao.AddMinutes(EXPIRY_MINUTES),
                giayConLai      = Math.Max(0, giayConLai),
                cuaHangId       = lichSu.CuaHangId,
                tenCuaHang      = lichSu.CuaHang?.TenCuaHang ?? "N/A",
                bankCode, bankAccountNo, bankAccountName, configured
            });
        }

        // ================================================================
        // 5. KIỂM TRA TRẠNG THÁI THANH TOÁN (manual / auto-poll)
        // ================================================================
        [HttpGet("check-payment/{id}")]
        public async Task<IActionResult> CheckPayment(int id)
        {
            int cuaHangId = GetCuaHangId();

            var lichSu = await _context.LichSuDangKys
                .FirstOrDefaultAsync(l => l.Id == id && l.CuaHangId == cuaHangId);

            if (lichSu == null) return NotFound(new { message = "Không tìm thấy đơn!" });

            if (lichSu.TrangThai == "DaThanhToan")
                return Ok(new { trangThai = "DaThanhToan", message = "✅ Thanh toán đã được xác nhận!" });

            if (lichSu.TrangThai == "DaHuy")
                return Ok(new { trangThai = "DaHuy", message = "Đơn đã bị hủy." });

            // Kiểm tra hết hạn
            if (lichSu.NgayTao.AddMinutes(EXPIRY_MINUTES) < DateTime.Now)
            {
                lichSu.TrangThai = "DaHuy";
                lichSu.GhiChu = $"Tự động hủy: quá {EXPIRY_MINUTES} phút không thanh toán";
                await _context.SaveChangesAsync();
                return Ok(new { trangThai = "DaHuy", message = "Đơn đã hết hạn (quá 10 phút)." });
            }

            var giayConLai = (int)(lichSu.NgayTao.AddMinutes(EXPIRY_MINUTES) - DateTime.Now).TotalSeconds;
            return Ok(new
            {
                trangThai  = "ChoThanhToan",
                giayConLai = Math.Max(0, giayConLai),
                message    = "Đang chờ thanh toán..."
            });
        }

        // ================================================================
        // 6. HỦY ĐƠN ĐANG CHỜ THANH TOÁN
        // ================================================================
        [HttpPost("cancel/{id}")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            int cuaHangId = GetCuaHangId();

            var lichSu = await _context.LichSuDangKys
                .FirstOrDefaultAsync(l => l.Id == id && l.CuaHangId == cuaHangId && l.TrangThai == "ChoThanhToan");

            if (lichSu == null)
                return NotFound(new { message = "Không tìm thấy đơn chờ thanh toán!" });

            lichSu.TrangThai = "DaHuy";
            lichSu.GhiChu = "Người dùng tự hủy";
            await _context.SaveChangesAsync();

            Log.Information("❌ CuaHang [{Id}] hủy đơn {MaGD}", cuaHangId, lichSu.MaGiaoDich);
            return Ok(new { message = "Đã hủy đơn hàng." });
        }

        // ================================================================
        // 7. LẤY CẤU HÌNH NGÂN HÀNG SUPERADMIN (public helper cho frontend)
        // ================================================================
        [HttpGet("payment-config")]
        public async Task<IActionResult> GetPaymentConfig()
        {
            var (bankCode, bankAccountNo, bankAccountName, configured) = await GetSystemBankAsync();
            return Ok(new { bankCode, bankAccountNo, bankAccountName, configured });
        }

        // ================================================================
        // 8. LỊCH SỬ THANH TOÁN
        // ================================================================
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            int cuaHangId = GetCuaHangId();

            var history = await _context.LichSuDangKys
                .Include(l => l.GoiDichVu)
                .Where(l => l.CuaHangId == cuaHangId)
                .OrderByDescending(l => l.NgayTao)
                .Select(l => new
                {
                    l.Id, l.NgayBatDau, l.NgayKetThuc,
                    l.SoTienThanhToan, l.TrangThai,
                    l.MaGiaoDich, l.NgayThanhToan, l.NgayTao, l.GhiChu,
                    tenGoi = l.GoiDichVu != null ? l.GoiDichVu.TenGoi : "N/A"
                })
                .ToListAsync();

            return Ok(history);
        }
    }

    public class PurchaseDto
    {
        public int GoiDichVuId { get; set; }
    }
}
