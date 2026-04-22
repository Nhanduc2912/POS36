using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using POS36.Api.Models;
using System.Linq;
using System.Threading.Tasks;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ThuChiController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ThuChiController(AppDbContext context) { _context = context; }

        private int GetCuaHangId()
        {
            var claim = User.FindFirst("CuaHangId");
            if (claim == null) throw new UnauthorizedAccessException("Token không hợp lệ");
            return int.Parse(claim.Value);
        }

        // ChuCuaHang và Admin đều được xem sổ quỹ
        [Authorize(Roles = "ChuCuaHang,Admin")]
        [HttpGet("danh-sach")]
        public async Task<IActionResult> GetDanhSach(
            [FromQuery] int chiNhanhId,
            [FromQuery] string? startDate = null,
            [FromQuery] string? endDate   = null)
        {
            int cuaHangId = GetCuaHangId();

            // Query cơ sở — không lọc ngày ở DB để tránh lỗi UTC vs Local
            var baseQuery = _context.PhieuThuChis.Where(p => p.CuaHangId == cuaHangId);

            // QUAN TRỌNG: Khi lọc theo chi nhánh, bao gồm cả phiếu có ChiNhanhId=0
            // (các bàn không gán KhuVuc sẽ tạo phiếu với ChiNhanhId=0)
            if (chiNhanhId > 0)
                baseQuery = baseQuery.Where(p => p.ChiNhanhId == chiNhanhId || p.ChiNhanhId == 0);

            // Tải toàn bộ về memory rồi so sánh ngày theo local time
            // (tránh lỗi DateTime UTC vs Local từ dữ liệu cũ được lưu bằng UtcNow)
            var allRecords = await baseQuery.OrderByDescending(p => p.NgayGiaoDich).ToListAsync();

            // Parse khoảng ngày lọc
            DateTime? startDt = null, endDt = null;
            if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var sd))
                startDt = sd.Date;
            if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var ed))
                endDt = ed.Date.AddDays(1).AddTicks(-1);

            // Hàm chuẩn hóa ngày về local (xử lý cả UTC lẫn Unspecified)
            DateTime ToLocal(DateTime dt) =>
                dt.Kind == DateTimeKind.Utc ? dt.ToLocalTime() : dt;

            // === SỐ DƯ ĐẦU KỲ = Tích lũy tất cả GD trước startDate ===
            double dauKy = 0;
            if (startDt.HasValue)
            {
                var truocKy = allRecords.Where(p => ToLocal(p.NgayGiaoDich).Date < startDt.Value.Date).ToList();
                dauKy = truocKy.Where(p => p.LoaiPhieu == "Thu").Sum(p => p.GiaTri)
                      - truocKy.Where(p => p.LoaiPhieu == "Chi").Sum(p => p.GiaTri);
            }

            // === LỌC GIAO DỊCH TRONG KỲ ===
            var danhSach = allRecords.Where(p =>
            {
                var ngayLocal = ToLocal(p.NgayGiaoDich).Date;
                if (startDt.HasValue && ngayLocal < startDt.Value.Date) return false;
                if (endDt.HasValue   && ngayLocal > endDt.Value.Date)   return false;
                return true;
            }).ToList();

            double tongThu = danhSach.Where(p => p.LoaiPhieu == "Thu").Sum(p => p.GiaTri);
            double tongChi = danhSach.Where(p => p.LoaiPhieu == "Chi").Sum(p => p.GiaTri);

            return Ok(new
            {
                ThongKe = new
                {
                    DauKy  = dauKy,
                    TongThu = tongThu,
                    TongChi = tongChi,
                    TonQuy  = dauKy + tongThu - tongChi
                },
                DanhSach = danhSach
            });
        }

        [Authorize(Roles = "Admin,ChuCuaHang,NhanVien")]
        [HttpPost]
        public async Task<IActionResult> TaoPhieuThuChi([FromBody] PhieuThuChiDto dto)
        {
            try
            {
                if (dto.GiaTri <= 0)
                    return BadRequest("Số tiền phải lớn hơn 0.");
                if (string.IsNullOrWhiteSpace(dto.LoaiPhieu) || (dto.LoaiPhieu != "Thu" && dto.LoaiPhieu != "Chi"))
                    return BadRequest("Loại phiếu không hợp lệ.");
                if (string.IsNullOrWhiteSpace(dto.PhuongThuc))
                    return BadRequest("Phương thức thanh toán là bắt buộc.");
                if (string.IsNullOrWhiteSpace(dto.NguoiNopNhan))
                    return BadRequest("Người nộp/nhận là bắt buộc.");

                int cuaHangId = GetCuaHangId();
                string prefix = dto.LoaiPhieu == "Thu" ? "PT" : "PC";
                string hangMucDefault = dto.LoaiPhieu == "Thu" ? "Thu khác" : "Chi khác";

                // Luôn dùng DateTime.Now (local time) để nhất quán với HoaDon
                DateTime ngayGD = DateTime.Now;
                if (!string.IsNullOrEmpty(dto.NgayGiaoDich) && DateTime.TryParse(dto.NgayGiaoDich, out var parsed))
                    ngayGD = parsed.Date.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute);

                var phieu = new PhieuThuChi
                {
                    CuaHangId    = cuaHangId,
                    ChiNhanhId   = dto.ChiNhanhId > 0 ? dto.ChiNhanhId : 1,
                    LoaiPhieu    = dto.LoaiPhieu,
                    PhuongThuc   = dto.PhuongThuc.Trim(),
                    NguoiNopNhan = dto.NguoiNopNhan.Trim(),
                    HangMuc      = !string.IsNullOrWhiteSpace(dto.HangMuc) ? dto.HangMuc.Trim() : hangMucDefault,
                    LyDo         = dto.LyDo?.Trim() ?? "",
                    GiaTri       = dto.GiaTri,
                    NgayGiaoDich = ngayGD,
                    NguoiTao     = User.Identity?.Name ?? "Admin",
                    MaChungTu    = prefix + DateTime.Now.ToString("yyMMddHHmmss")
                };

                _context.PhieuThuChis.Add(phieu);
                await _context.SaveChangesAsync();
                return Ok(phieu);
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi khi tạo phiếu: " + ex.Message);
            }
        }
    }

    public class PhieuThuChiDto
    {
        public int     ChiNhanhId    { get; set; }
        public string  LoaiPhieu     { get; set; } = string.Empty;
        public string  PhuongThuc    { get; set; } = string.Empty;
        public string  NguoiNopNhan  { get; set; } = string.Empty;
        public string  HangMuc       { get; set; } = string.Empty;
        public string  LyDo          { get; set; } = string.Empty;
        public double  GiaTri        { get; set; }
        public string? NgayGiaoDich  { get; set; } // "yyyy-MM-dd"
    }
}