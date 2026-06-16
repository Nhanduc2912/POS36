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

        /// <summary>
        /// Lấy danh sách các gói dịch vụ đang hoạt động
        /// </summary>
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

        /// <summary>
        /// Xem gói hiện tại + ngày hết hạn của cửa hàng
        /// </summary>
        [HttpGet("my-plan")]
        public async Task<IActionResult> GetMyPlan()
        {
            int cuaHangId = GetCuaHangId();
            var store = await _context.CuaHangs.FindAsync(cuaHangId);
            if (store == null) return NotFound();

            // Lấy thông tin gói hiện tại
            Models.GoiDichVu? currentPlan = null;
            if (!string.IsNullOrEmpty(store.GoiDichVu))
            {
                currentPlan = await _context.GoiDichVus
                    .FirstOrDefaultAsync(g => g.MaGoi == store.GoiDichVu);
            }

            // Đếm hóa đơn tháng này (cho gói Starter)
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

        /// <summary>
        /// Tạo đơn mua gói → sinh mã chuyển khoản cho SePay webhook
        /// </summary>
        [HttpPost("purchase")]
        public async Task<IActionResult> Purchase([FromBody] PurchaseDto dto)
        {
            int cuaHangId = GetCuaHangId();

            var goi = await _context.GoiDichVus.FindAsync(dto.GoiDichVuId);
            if (goi == null || !goi.IsActive)
                return BadRequest("Gói dịch vụ không tồn tại hoặc đã ngưng kinh doanh!");

            // Kiểm tra xem có đơn nào đang chờ thanh toán không
            var donChoTT = await _context.LichSuDangKys
                .FirstOrDefaultAsync(l => l.CuaHangId == cuaHangId && l.TrangThai == "ChoThanhToan");

            if (donChoTT != null)
            {
                // Trả lại đơn cũ chưa thanh toán thay vì tạo mới
                return Ok(new
                {
                    message = "Bạn đã có đơn đang chờ thanh toán!",
                    donDangKyId = donChoTT.Id,
                    maGiaoDich = donChoTT.MaGiaoDich,
                    tongTien = donChoTT.SoTienThanhToan,
                    tenGoi = goi.TenGoi
                });
            }

            // Tính ngày bắt đầu: nếu cửa hàng đang còn hạn → cộng dồn
            var store = await _context.CuaHangs.FindAsync(cuaHangId);
            DateTime ngayBatDau = (store!.NgayHetHan > DateTime.Now)
                ? store.NgayHetHan
                : DateTime.Now;
            DateTime ngayKetThuc = ngayBatDau.AddMonths(goi.SoThang);

            var lichSu = new Models.LichSuDangKy
            {
                CuaHangId = cuaHangId,
                GoiDichVuId = goi.Id,
                NgayBatDau = ngayBatDau,
                NgayKetThuc = ngayKetThuc,
                SoTienThanhToan = goi.TongGia,
                TrangThai = "ChoThanhToan",
                PhuongThucThanhToan = "SePay"
            };

            _context.LichSuDangKys.Add(lichSu);
            await _context.SaveChangesAsync();

            // Sinh mã giao dịch: POS36G{Id} — SePay webhook sẽ nhận diện mã này
            lichSu.MaGiaoDich = $"POS36G{lichSu.Id}";
            await _context.SaveChangesAsync();

            await _context.LogHoatDongAsync(int.Parse(User.FindFirst("ChiNhanhId")?.Value ?? "0"), "Nhật ký gói mua", $"Tạo yêu cầu thanh toán mua gói '{goi.TenGoi}' ({goi.SoThang} tháng) - Số tiền: {goi.TongGia:N0}đ. Mã giao dịch: {lichSu.MaGiaoDich}");

            Log.Information("💳 Cửa hàng [{CuaHangId}] tạo đơn mua gói {TenGoi} — Mã: {MaGiaoDich}",
                cuaHangId, goi.TenGoi, lichSu.MaGiaoDich);

            return Ok(new
            {
                message = "Đã tạo đơn đăng ký thành công! Vui lòng chuyển khoản theo thông tin bên dưới.",
                donDangKyId = lichSu.Id,
                maGiaoDich = lichSu.MaGiaoDich,
                tongTien = goi.TongGia,
                tenGoi = goi.TenGoi,
                soThang = goi.SoThang
            });
        }

        /// <summary>
        /// Lịch sử thanh toán
        /// </summary>
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
                    l.MaGiaoDich, l.NgayThanhToan, l.NgayTao,
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
