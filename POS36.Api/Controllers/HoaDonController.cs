using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using POS36.Api.DTOs;
using POS36.Api.Models;
using Microsoft.AspNetCore.SignalR; // 1. Thêm cái này
using POS36.Api.Hubs;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HoaDonController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<KitchenHub> _hubContext;

        public HoaDonController(AppDbContext context, IHubContext<KitchenHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        private int GetCuaHangId()
        {
            var claim = User.FindFirst("CuaHangId");
            if (claim == null) throw new UnauthorizedAccessException("Token không hợp lệ");
            return int.Parse(claim.Value);
        }

        [HttpPost("goimon")]
        public async Task<IActionResult> GoiMon(TaoDonHangDto request)
        {
            int cuaHangId = GetCuaHangId();

            // Bỏ qua kiểm tra CuaHangId để bàn mới tạo vẫn gọi món được
            var ban = await _context.Bans.Include(b => b.KhuVuc).FirstOrDefaultAsync(b => b.Id == request.BanId);
            if (ban == null) return BadRequest("Bàn không tồn tại trong hệ thống!");

            // 2. Chống lỗi Null khi danh sách món trống
            if (request.DanhSachMon == null || !request.DanhSachMon.Any())
                return BadRequest("Chưa có món nào được chọn!");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var hoaDon = await _context.HoaDons
                    .FirstOrDefaultAsync(h => h.BanId == request.BanId && h.TrangThai == "Đang phục vụ" && h.CuaHangId == cuaHangId);

                if (hoaDon == null)
                {
                    hoaDon = new HoaDon
                    {
                        CuaHangId = cuaHangId,

                        // FIX LỖI 500 Ở ĐÂY: Dùng dấu ? để an toàn. Nếu Bàn chưa có Khu vực thì tạm gán ChiNhanhId = 0
                        ChiNhanhId = ban.KhuVuc?.ChiNhanhId ?? 0,

                        BanId = request.BanId,
                        NgayTao = DateTime.Now,
                        TrangThai = "Đang phục vụ",
                        TongTien = 0
                    };
                    _context.HoaDons.Add(hoaDon);
                    await _context.SaveChangesAsync();

                    ban.TrangThai = "Đang phục vụ";
                }

                decimal tongTienCongThem = 0;

                foreach (var mon in request.DanhSachMon)
                {
                    var sanPham = await _context.SanPhams.FindAsync(mon.SanPhamId);
                    if (sanPham == null || sanPham.CuaHangId != cuaHangId)
                        throw new Exception($"Sản phẩm {mon.SanPhamId} không tồn tại!");

                    var chiTiet = new ChiTietHoaDon
                    {
                        HoaDonId = hoaDon.Id,
                        SanPhamId = mon.SanPhamId,
                        SoLuong = mon.SoLuong,
                        DonGia = sanPham.GiaBan,
                        GhiChu = mon.GhiChu ?? "", // Chống Null ghi chú
                        TrangThaiMon = "Chờ chế biến"
                    };

                    _context.ChiTietHoaDons.Add(chiTiet);
                    tongTienCongThem += (chiTiet.SoLuong * chiTiet.DonGia);
                }

                hoaDon.TongTien += tongTienCongThem;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Bắn SignalR xuống bếp
                if (_hubContext != null)
                {
                    await _hubContext.Clients.All.SendAsync("CoDonHangMoi", new
                    {
                        banId = request.BanId,
                        tenBan = ban.TenBan,
                        hoaDonId = hoaDon.Id,
                        thoiGian = DateTime.Now.ToString("HH:mm:ss")
                    });
                }

                return Ok(new { message = "Gọi món thành công! Đã gửi xuống bếp.", hoaDonId = hoaDon.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Lỗi hệ thống: {ex.Message}");
            }
        }

        // ==========================================
        // 2. LẤY CHI TIẾT HÓA ĐƠN CỦA 1 BÀN (DÀNH CHO ORDER & THU NGÂN)
        // ==========================================
        [HttpGet("ban/{banId}")]
        public async Task<IActionResult> GetHoaDonBan(int banId)
        {
            int cuaHangId = GetCuaHangId();

            var hoaDon = await _context.HoaDons
                .Include(h => h.ChiTietHoaDons!)
                .ThenInclude(ct => ct.SanPham)
                .FirstOrDefaultAsync(h => h.BanId == banId && h.TrangThai == "Đang phục vụ" && h.CuaHangId == cuaHangId);

            if (hoaDon == null) return NotFound("Bàn này chưa có hóa đơn mở!");

            var result = new
            {
                HoaDonId = hoaDon.Id,
                BanId = hoaDon.BanId,
                ThoiGianNgoiNghut = (int)(DateTime.Now - hoaDon.NgayTao).TotalMinutes,
                TongTien = hoaDon.TongTien,
                DanhSachMon = hoaDon.ChiTietHoaDons!.Select(ct => new
                {
                    ChiTietId = ct.Id,
                    TenSanPham = ct.SanPham?.TenSanPham,
                    SoLuong = ct.SoLuong,
                    DonGia = ct.DonGia,
                    ThanhTien = ct.SoLuong * ct.DonGia,
                    TrangThaiMon = ct.TrangThaiMon,
                    GhiChu = ct.GhiChu
                }).ToList()
            };

            return Ok(result);
        }

        // ==========================================
        // 3. CHUYỂN BÀN
        // ==========================================
        [HttpPost("chuyenban")]
        public async Task<IActionResult> ChuyenBan(ChuyenBanDto request)
        {
            int cuaHangId = GetCuaHangId();

            var tuBan = await _context.Bans.FirstOrDefaultAsync(b => b.Id == request.TuBanId && b.CuaHangId == cuaHangId);
            var denBan = await _context.Bans.FirstOrDefaultAsync(b => b.Id == request.DenBanId && b.CuaHangId == cuaHangId);

            if (tuBan == null || denBan == null) return BadRequest("Bàn không tồn tại.");
            if (tuBan.TrangThai != "Đang phục vụ") return BadRequest("Bàn chuyển đi chưa có khách.");
            if (denBan.TrangThai == "Đang phục vụ") return BadRequest("Bàn chuyển đến đã có khách, vui lòng sử dụng chức năng Ghép Bàn.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var hoaDon = await _context.HoaDons.FirstOrDefaultAsync(h => h.BanId == request.TuBanId && h.TrangThai == "Đang phục vụ" && h.CuaHangId == cuaHangId);
                if (hoaDon == null) return BadRequest("Không tìm thấy hóa đơn của bàn chuyển đi.");

                // Đổi bàn cho hóa đơn
                hoaDon.BanId = request.DenBanId;

                // Cập nhật trạng thái bàn
                tuBan.TrangThai = "Trống";
                denBan.TrangThai = "Đang phục vụ";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = $"Đã chuyển bàn thành công sang {denBan.TenBan}!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Lỗi hệ thống: {ex.Message}");
            }
        }

        // ==========================================
        // 4. GHÉP BÀN
        // ==========================================
        [HttpPost("ghepban")]
        public async Task<IActionResult> GhepBan(GhepBanDto request)
        {
            int cuaHangId = GetCuaHangId();

            var tuBan = await _context.Bans.FirstOrDefaultAsync(b => b.Id == request.TuBanId && b.CuaHangId == cuaHangId);
            var denBan = await _context.Bans.FirstOrDefaultAsync(b => b.Id == request.DenBanId && b.CuaHangId == cuaHangId);

            if (tuBan == null || denBan == null) return BadRequest("Bàn không tồn tại.");
            if (tuBan.TrangThai != "Đang phục vụ" || denBan.TrangThai != "Đang phục vụ")
                return BadRequest("Cả 2 bàn phải đang có khách để thực hiện ghép.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var hoaDonGoc = await _context.HoaDons.Include(h => h.ChiTietHoaDons)
                    .FirstOrDefaultAsync(h => h.BanId == request.TuBanId && h.TrangThai == "Đang phục vụ" && h.CuaHangId == cuaHangId);

                var hoaDonDich = await _context.HoaDons.Include(h => h.ChiTietHoaDons)
                    .FirstOrDefaultAsync(h => h.BanId == request.DenBanId && h.TrangThai == "Đang phục vụ" && h.CuaHangId == cuaHangId);

                if (hoaDonGoc == null || hoaDonDich == null) return BadRequest("Không tìm thấy thông tin hóa đơn.");

                // Chuyển toàn bộ chi tiết món từ Hóa Đơn Gốc sang Hóa Đơn Đích
                foreach (var chiTiet in hoaDonGoc.ChiTietHoaDons)
                {
                    chiTiet.HoaDonId = hoaDonDich.Id;
                }

                // Cập nhật tổng tiền hóa đơn đích
                hoaDonDich.TongTien += hoaDonGoc.TongTien;

                // Xóa hóa đơn gốc hoặc đánh dấu là Đã hủy/Ghép
                hoaDonGoc.TrangThai = "Đã Ghép Bàn";
                hoaDonGoc.TongTien = 0; // Để không bị tính double vào doanh thu nếu query sai

                // Cập nhật trạng thái bàn gốc
                tuBan.TrangThai = "Trống";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = $"Đã ghép bàn thành công vào {denBan.TenBan}!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Lỗi hệ thống: {ex.Message}");
            }
        }
        // ==========================================
        // 5. THANH TOÁN (THU NGÂN)
        // ==========================================
        [HttpPost("thanhtoan/{banId}")]
        public async Task<IActionResult> ThanhToan(int banId)
        {
            int cuaHangId = GetCuaHangId();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Tìm Hóa Đơn đang phục vụ của bàn này (bỏ CuaHangId)
                var hoaDon = await _context.HoaDons.FirstOrDefaultAsync(h => h.BanId == banId && h.TrangThai == "Đang phục vụ");
                var ban = await _context.Bans.FirstOrDefaultAsync(b => b.Id == banId);

                if (hoaDon == null || ban == null) return BadRequest("Không tìm thấy thông tin bàn/hóa đơn!");

                // Chốt hóa đơn & Giải phóng bàn
                hoaDon.TrangThai = "Đã thanh toán";
                ban.TrangThai = "Trống";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Thanh toán thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi: " + ex.Message);
            }
        }

        // ==========================================
        // 6. LẤY MÓN CHỜ CHẾ BIẾN (CHO MÀN HÌNH BẾP)
        // ==========================================
        // ==========================================
        // 6. LẤY MÓN CHỜ CHẾ BIẾN (CHO MÀN HÌNH BẾP) - ĐÃ FIX LỖI 500
        // ==========================================
        [HttpGet("bep/danh-sach")]
        public async Task<IActionResult> GetMonChoBep([FromQuery] int chiNhanhId)
        {
            try
            {
                int cuaHangId = GetCuaHangId();

                // BƯỚC 1: Lấy dữ liệu thô từ Database lên RAM (chưa tính thời gian vội để tránh lỗi EF Core)
                var rawItems = await _context.ChiTietHoaDons
                    .Where(c => c.HoaDon != null
                                && c.HoaDon.ChiNhanhId == chiNhanhId
                                && c.HoaDon.CuaHangId == cuaHangId
                                && c.HoaDon.TrangThai == "Đang phục vụ"
                                && c.TrangThaiMon == "Chờ chế biến")
                    .Select(c => new
                    {
                        ChiTietId = c.Id,
                        BanId = c.HoaDon!.BanId,
                        // Bọc thêm phát nữa nhỡ Bàn hoặc Món ăn bị xóa khỏi DB
                        TenBan = c.HoaDon.Ban != null ? c.HoaDon.Ban.TenBan : "Bàn ảo",
                        TenMon = c.SanPham != null ? c.SanPham.TenSanPham : "Món đã bị xóa",
                        SoLuong = c.SoLuong,
                        GhiChu = c.GhiChu,
                        NgayTao = c.HoaDon.NgayTao // Kéo nguyên gốc ngày tạo lên
                    })
                    .ToListAsync();

                // BƯỚC 2: Dùng C# trên RAM để tính ra số phút chờ
                var items = rawItems.Select(c => new
                {
                    c.ChiTietId,
                    c.BanId,
                    c.TenBan,
                    c.TenMon,
                    c.SoLuong,
                    c.GhiChu,
                    ThoiGianCho = (int)(DateTime.Now - c.NgayTao).TotalMinutes
                })
                .OrderByDescending(c => c.ThoiGianCho) // Món nào chờ lâu nhất cho lên đầu
                .ToList();

                return Ok(items);
            }
            catch (Exception ex)
            {
                // Nhỡ có lỗi gì khác thì nó in rõ ra màn hình Terminal C# cho em dễ đọc
                Console.WriteLine("LỖI LẤY MÓN BẾP: " + ex.Message);
                return StatusCode(500, "Lỗi server: " + ex.Message);
            }
        }

        // ==========================================
        // 7. BẾP XÁC NHẬN ĐÃ LÀM XONG
        // ==========================================
        [HttpPut("bep/xong/{chiTietId}")]
        public async Task<IActionResult> MonDaXong(int chiTietId)
        {
            var chiTiet = await _context.ChiTietHoaDons.Include(c => c.HoaDon).ThenInclude(h => h.Ban).FirstOrDefaultAsync(c => c.Id == chiTietId);
            if (chiTiet == null) return NotFound();

            chiTiet.TrangThaiMon = "Đã Xong";
            await _context.SaveChangesAsync();

            // Bắn tín hiệu lại cho Order biết ra bưng đồ
            await _hubContext.Clients.All.SendAsync("MonAnDaXong", new
            {
                banId = chiTiet.HoaDon!.BanId,
                tenBan = chiTiet.HoaDon.Ban!.TenBan,
                tenMon = chiTiet.SoLuong + "x Đã xong"
            });

            return Ok();
        }
    }
}