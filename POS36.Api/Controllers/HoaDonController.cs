using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using POS36.Api.DTOs;
using POS36.Api.Models;
using Microsoft.AspNetCore.SignalR;
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
        [Authorize(Roles = "SuperAdmin,ChuCuaHang,Admin,QuanLy,ThuNgan,Order")]
        public async Task<IActionResult> GoiMon(TaoDonHangDto request)
        {
            int cuaHangId = GetCuaHangId();

            // BUG-07 FIX: Kiểm tra CuaHangId để chống multi-tenant leak
            var ban = await _context.Bans.Include(b => b.KhuVuc)
                .FirstOrDefaultAsync(b => b.Id == request.BanId && b.CuaHangId == cuaHangId);
            if (ban == null) return BadRequest("Bàn không tồn tại trong cửa hàng của bạn!");

            var branchClaim = User.FindFirst("ChiNhanhId");
            if (branchClaim != null)
            {
                int userBranchId = int.Parse(branchClaim.Value);
                if (ban.KhuVuc?.ChiNhanhId != userBranchId)
                {
                    return StatusCode(403, "Bạn không có quyền thực hiện nghiệp vụ cho chi nhánh khác!");
                }
            }

            // Chống lỗi Null khi danh sách món trống
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

                    // BUG #6 FIX: Kiểm tra tồn kho trước khi cho gọi món
                    if (sanPham.TrangThai == false)
                        throw new Exception($"'{sanPham.TenSanPham}' hiện đã ngừng bán!");

                    // BUG-06 FIX: Sử dụng pessimistic locking WITH (UPDLOCK, ROWLOCK) để tránh race condition
                    var tonKho = await _context.TonKhos
                        .FromSqlRaw("SELECT * FROM TonKhos WITH (UPDLOCK, ROWLOCK) WHERE SanPhamId = {0} AND ChiNhanhId = {1}", mon.SanPhamId, hoaDon.ChiNhanhId)
                        .FirstOrDefaultAsync();

                    bool isChoPhepBanAm = await GetThietLapBoolAsync(cuaHangId, "Kho_ChoPhepBanAm", true);

                    if (!isChoPhepBanAm)
                    {
                        if (tonKho != null && tonKho.SoLuong < mon.SoLuong)
                            throw new Exception($"'{sanPham.TenSanPham}' chỉ còn {tonKho.SoLuong} trong kho, không đủ để gọi {mon.SoLuong}!");
                        if (tonKho == null)
                            throw new Exception($"'{sanPham.TenSanPham}' chưa có trong kho, không thể gọi món!");
                    }

                    decimal currentGiaVon = await _context.ChiTietPhieuNhaps
                        .Where(ct => ct.SanPhamId == mon.SanPhamId && ct.PhieuNhap != null && ct.PhieuNhap.ChiNhanhId == hoaDon.ChiNhanhId && ct.PhieuNhap.TrangThai == "Hoàn thành")
                        .Select(ct => (decimal?)ct.DonGiaNhap)
                        .AverageAsync() ?? 0;

                    var chiTiet = new ChiTietHoaDon
                    {
                        HoaDonId = hoaDon.Id,
                        SanPhamId = mon.SanPhamId,
                        SoLuong = mon.SoLuong,
                        DonGia = sanPham.GiaBan,
                        GiaVon = currentGiaVon,
                        GhiChu = mon.GhiChu ?? "",
                        TrangThaiMon = "Chờ chế biến"
                    };

                    _context.ChiTietHoaDons.Add(chiTiet);
                    tongTienCongThem += (chiTiet.SoLuong * chiTiet.DonGia);
                }

                if (!string.IsNullOrWhiteSpace(request.GhiChuDot))
                {
                    hoaDon.GhiChu = string.IsNullOrWhiteSpace(hoaDon.GhiChu)
                        ? request.GhiChuDot
                        : hoaDon.GhiChu + " | " + request.GhiChuDot;
                }

                hoaDon.TongTien += tongTienCongThem;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Ghi nhận nhật ký hoạt động
                await _context.LogHoatDongAsync(ban.KhuVuc?.ChiNhanhId ?? 0, "Gọi món", $"Đặt món cho {ban.TenBan}. Số món: {request.DanhSachMon.Count}, tăng tổng bill thêm {tongTienCongThem:N0}đ.");

                if (_hubContext != null)
                {
                    await _hubContext.Clients.Group($"store_{cuaHangId}").SendAsync("CoDonHangMoi", new
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
        [Authorize(Roles = "SuperAdmin,ChuCuaHang,Admin,QuanLy,ThuNgan,Order")]
        public async Task<IActionResult> GetHoaDonBan(int banId)
        {
            int cuaHangId = GetCuaHangId();

            var hoaDon = await _context.HoaDons
                .Include(h => h.ChiTietHoaDons!)
                .ThenInclude(ct => ct.SanPham)
                .FirstOrDefaultAsync(h => h.BanId == banId && h.TrangThai == "Đang phục vụ" && h.CuaHangId == cuaHangId);

            if (hoaDon == null)
            {
                // Fallback: Tìm hóa đơn đã thanh toán gần nhất trong vòng 30 phút của bàn này
                hoaDon = await _context.HoaDons
                    .Include(h => h.ChiTietHoaDons!)
                    .ThenInclude(ct => ct.SanPham)
                    .Where(h => h.BanId == banId && h.TrangThai == "Đã thanh toán" && h.CuaHangId == cuaHangId)
                    .OrderByDescending(h => h.NgayThanhToan)
                    .FirstOrDefaultAsync();

                if (hoaDon == null || (DateTime.Now - hoaDon.NgayThanhToan.GetValueOrDefault()).TotalMinutes > 30)
                {
                    return NotFound("Bàn này chưa có hóa đơn mở!");
                }
            }

            var result = new
            {
                HoaDonId = hoaDon.Id,
                BanId = hoaDon.BanId,
                ThoiGianNgoiNghut = (int)(DateTime.Now - hoaDon.NgayTao).TotalMinutes,
                TongTien = hoaDon.TongTien,
                GhiChu = hoaDon.GhiChu,
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
        [Authorize(Roles = "SuperAdmin,ChuCuaHang,Admin,QuanLy,ThuNgan,Order")]
        public async Task<IActionResult> ChuyenBan(ChuyenBanDto request)
        {
            int cuaHangId = GetCuaHangId();

            // BUG-11/Perm check: Nhân viên Order chuyển bàn
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? User.FindFirst("VaiTro")?.Value;
            if (role == "Order")
            {
                var isPermitted = await GetThietLapBoolAsync(cuaHangId, "Perm_Order_ChuyenTach", true);
                if (!isPermitted)
                    return StatusCode(403, "Nhân viên Order không được cấp quyền chuyển bàn!");
            }

            var tuBan = await _context.Bans.Include(b => b.KhuVuc).FirstOrDefaultAsync(b => b.Id == request.TuBanId && b.CuaHangId == cuaHangId);
            var denBan = await _context.Bans.Include(b => b.KhuVuc).FirstOrDefaultAsync(b => b.Id == request.DenBanId && b.CuaHangId == cuaHangId);

            if (tuBan == null || denBan == null) return BadRequest("Bàn không tồn tại.");

            var branchClaim = User.FindFirst("ChiNhanhId");
            if (branchClaim != null)
            {
                int userBranchId = int.Parse(branchClaim.Value);
                if (tuBan.KhuVuc?.ChiNhanhId != userBranchId || denBan.KhuVuc?.ChiNhanhId != userBranchId)
                {
                    return StatusCode(403, "Bạn không có quyền thực hiện nghiệp vụ cho chi nhánh khác!");
                }
            }
            if (tuBan.TrangThai != "Đang phục vụ") return BadRequest("Bàn chuyển đi chưa có khách.");
            if (denBan.TrangThai == "Đang phục vụ") return BadRequest("Bàn chuyển đến đã có khách, vui lòng sử dụng chức năng Ghép Bàn.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var hoaDon = await _context.HoaDons.FirstOrDefaultAsync(h => h.BanId == request.TuBanId && h.TrangThai == "Đang phục vụ" && h.CuaHangId == cuaHangId);
                if (hoaDon == null) return BadRequest("Không tìm thấy hóa đơn của bàn chuyển đi.");

                hoaDon.BanId = request.DenBanId;

                tuBan.TrangThai = "Trống";
                denBan.TrangThai = "Đang phục vụ";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Ghi nhận nhật ký hoạt động
                await _context.LogHoatDongAsync(tuBan.KhuVuc?.ChiNhanhId ?? 0, "Chuyển bàn", $"Chuyển bàn từ {tuBan.TenBan} sang {denBan.TenBan}. Hóa đơn chuyển: {hoaDon.TongTien:N0}đ");

                // Phát SignalR cho tất cả màn hình cùng cửa hàng cập nhật sơ đồ bàn
                if (_hubContext != null)
                {
                    await _hubContext.Clients.Group($"store_{cuaHangId}").SendAsync("CapNhatBan", new
                    {
                        message = "Chuyển bàn",
                        tuBan = tuBan.TenBan,
                        denBan = denBan.TenBan
                    });
                }

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
        [Authorize(Roles = "SuperAdmin,ChuCuaHang,Admin,QuanLy,ThuNgan,Order")]
        public async Task<IActionResult> GhepBan(GhepBanDto request)
        {
            int cuaHangId = GetCuaHangId();

            // BUG-11/Perm check: Nhân viên Order ghép bàn
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? User.FindFirst("VaiTro")?.Value;
            if (role == "Order")
            {
                var isPermitted = await GetThietLapBoolAsync(cuaHangId, "Perm_Order_ChuyenTach", true);
                if (!isPermitted)
                    return StatusCode(403, "Nhân viên Order không được cấp quyền ghép bàn!");
            }

            var tuBan = await _context.Bans.Include(b => b.KhuVuc).FirstOrDefaultAsync(b => b.Id == request.TuBanId && b.CuaHangId == cuaHangId);
            var denBan = await _context.Bans.Include(b => b.KhuVuc).FirstOrDefaultAsync(b => b.Id == request.DenBanId && b.CuaHangId == cuaHangId);

            if (tuBan == null || denBan == null) return BadRequest("Bàn không tồn tại.");

            var branchClaim = User.FindFirst("ChiNhanhId");
            if (branchClaim != null)
            {
                int userBranchId = int.Parse(branchClaim.Value);
                if (tuBan.KhuVuc?.ChiNhanhId != userBranchId || denBan.KhuVuc?.ChiNhanhId != userBranchId)
                {
                    return StatusCode(403, "Bạn không có quyền thực hiện nghiệp vụ cho chi nhánh khác!");
                }
            }
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

                foreach (var chiTiet in hoaDonGoc.ChiTietHoaDons)
                {
                    chiTiet.HoaDonId = hoaDonDich.Id;
                }

                hoaDonDich.TongTien += hoaDonGoc.TongTien;

                // BUG #10 FIX: Kế thừa KhachHangId từ bàn gốc nếu bàn đích chưa có khách
                if (hoaDonDich.KhachHangId == null && hoaDonGoc.KhachHangId != null)
                {
                    hoaDonDich.KhachHangId = hoaDonGoc.KhachHangId;
                }

                hoaDonGoc.TrangThai = "Đã Ghép Bàn";
                hoaDonGoc.TongTien = 0;

                tuBan.TrangThai = "Trống";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Ghi nhận nhật ký hoạt động
                await _context.LogHoatDongAsync(tuBan.KhuVuc?.ChiNhanhId ?? 0, "Ghép bàn", $"Ghép bàn từ {tuBan.TenBan} vào {denBan.TenBan}");

                // Phát SignalR cho tất cả màn hình cùng cửa hàng cập nhật sơ đồ bàn
                if (_hubContext != null)
                {
                    await _hubContext.Clients.Group($"store_{cuaHangId}").SendAsync("CapNhatBan", new
                    {
                        message = "Ghép bàn",
                        tuBan = tuBan.TenBan,
                        denBan = denBan.TenBan
                    });
                }

                return Ok(new { message = $"Đã ghép bàn thành công vào {denBan.TenBan}!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Lỗi hệ thống: {ex.Message}");
            }
        }

        // ==========================================
        // 4b. TÁCH BÀN — Tách một số món sang bàn trống khác
        // ==========================================
        [HttpPost("tachban")]
        [Authorize(Roles = "SuperAdmin,ChuCuaHang,Admin,QuanLy,ThuNgan,Order")]
        public async Task<IActionResult> TachBan(TachBanDto request)
        {
            int cuaHangId = GetCuaHangId();

            // BUG-11/Perm check: Nhân viên Order tách bàn
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? User.FindFirst("VaiTro")?.Value;
            if (role == "Order")
            {
                var isPermitted = await GetThietLapBoolAsync(cuaHangId, "Perm_Order_ChuyenTach", true);
                if (!isPermitted)
                    return StatusCode(403, "Nhân viên Order không được cấp quyền tách bàn!");
            }

            var tuBan = await _context.Bans.Include(b => b.KhuVuc).FirstOrDefaultAsync(b => b.Id == request.TuBanId && b.CuaHangId == cuaHangId);
            var denBan = await _context.Bans.Include(b => b.KhuVuc).FirstOrDefaultAsync(b => b.Id == request.DenBanId && b.CuaHangId == cuaHangId);

            if (tuBan == null || denBan == null) return BadRequest("Bàn không tồn tại.");

            var branchClaim = User.FindFirst("ChiNhanhId");
            if (branchClaim != null)
            {
                int userBranchId = int.Parse(branchClaim.Value);
                if (tuBan.KhuVuc?.ChiNhanhId != userBranchId || denBan.KhuVuc?.ChiNhanhId != userBranchId)
                {
                    return StatusCode(403, "Bạn không có quyền thực hiện nghiệp vụ cho chi nhánh khác!");
                }
            }
            if (tuBan.TrangThai != "Đang phục vụ") return BadRequest("Bàn nguồn chưa có khách.");
            if (denBan.TrangThai == "Đang phục vụ") return BadRequest("Bàn đích đang có khách, vui lòng chọn bàn trống.");
            if (request.DanhSachChiTietId == null || !request.DanhSachChiTietId.Any())
                return BadRequest("Chưa chọn món nào để tách.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var hoaDonGoc = await _context.HoaDons
                    .Include(h => h.ChiTietHoaDons!)
                    .ThenInclude(ct => ct.SanPham)
                    .FirstOrDefaultAsync(h => h.BanId == request.TuBanId && h.TrangThai == "Đang phục vụ" && h.CuaHangId == cuaHangId);

                if (hoaDonGoc == null) return BadRequest("Không tìm thấy hóa đơn bàn nguồn.");

                // Lấy các chi tiết cần tách
                var monTach = hoaDonGoc.ChiTietHoaDons
                    .Where(ct => request.DanhSachChiTietId.Contains(ct.Id))
                    .ToList();

                if (!monTach.Any()) return BadRequest("Không tìm thấy món cần tách trong hóa đơn.");

                decimal tongTienTach = monTach.Sum(ct => ct.DonGia * ct.SoLuong);

                // Tạo hóa đơn mới cho bàn đích
                var hoaDonMoi = new HoaDon
                {
                    CuaHangId = cuaHangId,
                    ChiNhanhId = hoaDonGoc.ChiNhanhId,
                    BanId = request.DenBanId,
                    NgayTao = DateTime.Now,
                    TrangThai = "Đang phục vụ",
                    TongTien = tongTienTach
                };
                _context.HoaDons.Add(hoaDonMoi);
                await _context.SaveChangesAsync(); // Để lấy Id

                // Chuyển các chi tiết sang hóa đơn mới
                foreach (var ct in monTach)
                {
                    ct.HoaDonId = hoaDonMoi.Id;
                }

                hoaDonGoc.TongTien -= tongTienTach;
                denBan.TrangThai = "Đang phục vụ";

                // Nếu bàn gốc không còn món nào thì đặt thành trống
                var conLai = hoaDonGoc.ChiTietHoaDons.Except(monTach).ToList();
                if (!conLai.Any())
                {
                    hoaDonGoc.TrangThai = "Đã hủy";
                    tuBan.TrangThai = "Trống";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Ghi nhận nhật ký hoạt động chi tiết
                string chiTietCacMon = string.Join(", ", monTach.Select(ct => $"{ct.SoLuong}x {ct.SanPham?.TenSanPham ?? "Món"}"));
                await _context.LogHoatDongAsync(tuBan.KhuVuc?.ChiNhanhId ?? 0, "Tách bàn", $"Tách bàn từ {tuBan.TenBan} sang {denBan.TenBan}. Món tách: {chiTietCacMon}. Tổng tiền tách: {tongTienTach:N0}đ");

                // Dùng "CapNhatBan" thay vì "CoDonHangMoi" để Bếp KHÔNG hiện "có món mới"
                if (_hubContext != null)
                    await _hubContext.Clients.Group($"store_{cuaHangId}").SendAsync("CapNhatBan", new
                    {
                        message = "Tách bàn",
                        tuBan = tuBan.TenBan,
                        denBan = denBan.TenBan
                    });

                return Ok(new { message = $"Đã tách bàn thành công sang {denBan.TenBan}!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Lỗi hệ thống: {ex.Message}");
            }
        }


        // ==========================================
        // 5. THANH TOÁN KẾT HỢP TRỪ KHO & TẠO PHIẾU THU SỔ QUỸ
        // ==========================================
        [HttpPost("thanhtoan/{banId}")]
        [Authorize(Roles = "SuperAdmin,ChuCuaHang,Admin,QuanLy,ThuNgan,Order")]
        public async Task<IActionResult> ThanhToan(int banId,
            [FromQuery] string phuongThuc = "Tiền mặt",
            [FromQuery] int? khachHangId = null,
            [FromQuery] int diemSuDung = 0,
            [FromQuery] decimal discountPercent = 0) // FEAT-4: chiết khấu %
        {
            int cuaHangId = GetCuaHangId();
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var hoaDon = await _context.HoaDons
                    .Include(h => h.ChiTietHoaDons)
                    .Include(h => h.Ban)
                    .FirstOrDefaultAsync(h => h.BanId == banId && h.TrangThai == "Đang phục vụ" && h.CuaHangId == cuaHangId);

                if (hoaDon == null) return BadRequest("Không tìm thấy hóa đơn đang phục vụ của bàn này!");

                var branchClaim = User.FindFirst("ChiNhanhId");
                if (branchClaim != null)
                {
                    int userBranchId = int.Parse(branchClaim.Value);
                    if (hoaDon.ChiNhanhId != userBranchId)
                    {
                        return StatusCode(403, "Bạn không có quyền thực hiện nghiệp vụ cho chi nhánh khác!");
                    }
                }

                // FEAT-4: Áp chiết khấu % trước khi tính toán
                if (discountPercent > 0)
                {
                    // QuanLy trở lên mới được chiết khấu > 30%
                    var roleCheck = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                                 ?? User.FindFirst("VaiTro")?.Value;
                    if (discountPercent > 30 && roleCheck == "ThuNgan")
                        return StatusCode(403, "Thu ngân chỉ được chiết khấu tối đa 30%. Liên hệ Quản lý!");
                    if (discountPercent > 100)
                        return BadRequest("Chiết khấu không được vượt quá 100%!");

                    decimal tienGiamDiscount = hoaDon.TongTien * discountPercent / 100;
                    hoaDon.TongTien -= tienGiamDiscount;
                    if (hoaDon.TongTien < 0) hoaDon.TongTien = 0;
                }

                // CẬP NHẬT TRẠNG THÁI & PHƯƠNG THỨC TRƯỚC KHI GỌI SAVECHANGES
                hoaDon.TrangThai = "Đã thanh toán";
                hoaDon.NgayThanhToan = DateTime.Now;
                hoaDon.PhuongThucThanhToan = phuongThuc;

                if (hoaDon.Ban != null)
                {
                    hoaDon.Ban.TrangThai = "Trống";
                }

                if (hoaDon.ChiTietHoaDons != null)
                {
                    foreach (var chiTiet in hoaDon.ChiTietHoaDons)
                    {
                        // BUG-06 FIX: Sử dụng pessimistic locking WITH (UPDLOCK, ROWLOCK)
                        var tonKho = await _context.TonKhos
                            .FromSqlRaw("SELECT * FROM TonKhos WITH (UPDLOCK, ROWLOCK) WHERE SanPhamId = {0} AND ChiNhanhId = {1}", chiTiet.SanPhamId, hoaDon.ChiNhanhId)
                            .FirstOrDefaultAsync();

                        bool isChoPhepBanAm = await GetThietLapBoolAsync(cuaHangId, "Kho_ChoPhepBanAm", true);
                        if (tonKho != null)
                        {
                            tonKho.SoLuong -= chiTiet.SoLuong;
                            if (!isChoPhepBanAm && tonKho.SoLuong < 0)
                            {
                                throw new Exception($"'{chiTiet.SanPham?.TenSanPham ?? "Món ăn"}' không đủ tồn kho để thanh toán!");
                            }
                        }
                        else
                        {
                            if (!isChoPhepBanAm)
                            {
                                throw new Exception($"'{chiTiet.SanPham?.TenSanPham ?? "Món ăn"}' chưa có trong kho, không thể thanh toán!");
                            }
                            
                            // Tạo tồn kho âm nếu được phép
                            _context.TonKhos.Add(new TonKho
                            {
                                SanPhamId = chiTiet.SanPhamId,
                                ChiNhanhId = hoaDon.ChiNhanhId,
                                SoLuong = -chiTiet.SoLuong
                            });
                        }
                    }
                }

                // Mặc định tên khách trên phiếu thu
                // FEAT-1: Đọc tỷ lệ tích điểm từ ThietLap thay vì hardcode
                // Loyalty_TiLeDoiDiem: 1 điểm = bao nhiêu tiền (default 1000 = 1.000₫)
                // Loyalty_TiLeKiem    : cần bao nhiêu tiền để tích 1 điểm (default 20000)
                decimal tyLeQuyDoi = 1000m;   // Mặc định 1 điểm = 1.000₫
                decimal tyLeTichDiem = 20000m; // Mặc định 20.000₫ = 1 điểm
                var tlQuyDoi = await _context.ThietLaps
                    .FirstOrDefaultAsync(t => t.CuaHangId == cuaHangId && t.MaThietLap == "Loyalty_TiLeDoiDiem");
                var tlTichDiem = await _context.ThietLaps
                    .FirstOrDefaultAsync(t => t.CuaHangId == cuaHangId && t.MaThietLap == "Loyalty_TiLeKiem");
                if (tlQuyDoi != null && decimal.TryParse(tlQuyDoi.DuLieu, out decimal parsedQuyDoi) && parsedQuyDoi > 0)
                    tyLeQuyDoi = parsedQuyDoi;
                if (tlTichDiem != null && decimal.TryParse(tlTichDiem.DuLieu, out decimal parsedTichDiem) && parsedTichDiem > 0)
                    tyLeTichDiem = parsedTichDiem;

                string tenKhachHang = "Khách hàng lẻ";
                int diemCong = 0;
                decimal tienGiam = 0;

                // ============ XỬ LÝ KHÁCH HÀNG, TIÊU ĐIỂM & TÍCH ĐIỂM ============
                if (khachHangId.HasValue)
                {
                    var khachHang = await _context.KhachHangs
                        .FirstOrDefaultAsync(k => k.Id == khachHangId.Value && k.CuaHangId == cuaHangId);

                    if (khachHang != null)
                    {
                        hoaDon.KhachHangId = khachHang.Id;
                        tenKhachHang = khachHang.TenKhachHang;

                        // BƯỚC 1: XỬ LÝ TIÊU ĐIỂM (trước khi tính tích điểm)
                        if (diemSuDung > 0)
                        {
                            if (diemSuDung > khachHang.DiemHienTai)
                                return BadRequest(new { message = $"Khách chỉ có {khachHang.DiemHienTai} điểm, không đủ để sử dụng {diemSuDung} điểm!" });

                            tienGiam = diemSuDung * tyLeQuyDoi; // FEAT-1: Dùng tỷ lệ từ ThietLap
                            if (tienGiam > hoaDon.TongTien) tienGiam = hoaDon.TongTien;

                            khachHang.DiemHienTai -= diemSuDung;
                            hoaDon.TongTien -= tienGiam;
                            if (hoaDon.TongTien < 0) hoaDon.TongTien = 0;
                        }

                        // BƯỚC 2: TÍCH ĐIỂM từ số tiền thực tế thanh toán
                        // FEAT-1: Dùng tỷ lệ từ ThietLap thay vì hardcode 20.000
                        diemCong = (int)(hoaDon.TongTien / tyLeTichDiem);
                        khachHang.TongDiemTichLuy += diemCong;
                        khachHang.DiemHienTai += diemCong;
                    }
                }

                var phieuThu = new PhieuThuChi
                {
                    CuaHangId = hoaDon.CuaHangId,
                    ChiNhanhId = hoaDon.ChiNhanhId,
                    MaChungTu = $"PT{DateTime.Now:ddMMyy}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
                    LoaiPhieu = "Thu",
                    PhuongThuc = phuongThuc,
                    NguoiNopNhan = tenKhachHang,
                    HangMuc = "Thu tiền bán hàng",
                    LyDo = tienGiam > 0
                        ? $"Thanh toán hóa đơn cho {hoaDon.Ban?.TenBan ?? "Bàn ảo"} (Dùng {diemSuDung} điểm, giảm {tienGiam:N0}đ)"
                        : $"Thanh toán hóa đơn cho {hoaDon.Ban?.TenBan ?? "Bàn ảo"}",
                    GiaTri = (double)hoaDon.TongTien, // Số tiền thực thu sau giảm
                    NgayGiaoDich = DateTime.Now,
                    NguoiTao = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value ?? "Thu ngân"
                };
                _context.PhieuThuChis.Add(phieuThu);

                // LƯU DB
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Ghi nhận nhật ký hoạt động
                await _context.LogHoatDongAsync(hoaDon.ChiNhanhId, "Thanh toán", $"Thanh toán hóa đơn bàn {hoaDon.Ban?.TenBan}. Số tiền thực thu: {hoaDon.TongTien:N0}đ. Phương thức: {phuongThuc}");

                // Kích hoạt fetchTables bên Vue tải lại màu sắc của Bàn
                if (_hubContext != null)
                {
                    await _hubContext.Clients.Group($"store_{cuaHangId}").SendAsync("CapNhatBan", new { message = "Đã thanh toán" });
                }

                return Ok(new { message = "Thanh toán thành công!", diemCong, tienGiam });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi khi thanh toán: " + ex.Message);
            }
        }

        // ==========================================
        // 6. LẤY MÓN CHỜ CHẾ BIẾN (CHO MÀN HÌNH BẾP)
        // ==========================================
        [HttpGet("bep/danh-sach")]
        [Authorize(Roles = "SuperAdmin,ChuCuaHang,Admin,QuanLy,Bep")]
        public async Task<IActionResult> GetMonChoBep([FromQuery] int chiNhanhId)
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

                var rawItems = await _context.ChiTietHoaDons
                    .Where(c => c.HoaDon != null
                                && c.HoaDon.ChiNhanhId == chiNhanhId
                                && c.HoaDon.CuaHangId == cuaHangId
                                && (c.HoaDon.TrangThai == "Đang phục vụ" || c.HoaDon.TrangThai == "Đã thanh toán")
                                && c.TrangThaiMon == "Chờ chế biến")
                    .Select(c => new
                    {
                        ChiTietId = c.Id,
                        BanId = c.HoaDon!.BanId,
                        TenBan = c.HoaDon.Ban != null ? c.HoaDon.Ban.TenBan : "Mang về",
                        TenMon = c.SanPham != null ? c.SanPham.TenSanPham : "Món đã bị xóa",
                        SoLuong = c.SoLuong,
                        GhiChu = c.GhiChu,
                        NgayTao = c.HoaDon.NgayTao,
                        GhiChuHoaDon = c.HoaDon.GhiChu
                    })
                    .ToListAsync();

                var items = rawItems.Select(c => new
                {
                    c.ChiTietId,
                    c.BanId,
                    c.TenBan,
                    c.TenMon,
                    c.SoLuong,
                    c.GhiChu,
                    c.GhiChuHoaDon,
                    ThoiGianCho = (int)(DateTime.Now - c.NgayTao).TotalMinutes
                })
                .OrderByDescending(c => c.ThoiGianCho)
                .ToList();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi server: " + ex.Message);
            }
        }

        // ==========================================
        // 7. BẾP XÁC NHẬN ĐÃ LÀM XONG
        // ==========================================
        [HttpPut("bep/xong/{chiTietId}")]
        [Authorize(Roles = "SuperAdmin,ChuCuaHang,Admin,QuanLy,Bep")]
        public async Task<IActionResult> MonDaXong(int chiTietId)
        {
            int cuaHangId = GetCuaHangId();

            var chiTiet = await _context.ChiTietHoaDons
                .Include(c => c.SanPham)
                .Include(c => c.HoaDon).ThenInclude(h => h.Ban)
                .FirstOrDefaultAsync(c => c.Id == chiTietId);
            if (chiTiet == null) return NotFound();

            // BUG-08 FIX: Kiểm tra CuaHangId để chống tenant leak
            if (chiTiet.HoaDon == null || chiTiet.HoaDon.CuaHangId != cuaHangId)
                return StatusCode(403, "Bạn không có quyền thao tác trên hóa đơn này!");

            chiTiet.TrangThaiMon = "Đã Xong";
            await _context.SaveChangesAsync();

            var tenMon = chiTiet.SanPham?.TenSanPham ?? "Món ăn";

            // Ghi nhận nhật ký hoạt động
            await _context.LogHoatDongAsync(chiTiet.HoaDon?.ChiNhanhId ?? 0, "Báo xong món", $"Bếp báo xong món: {chiTiet.SoLuong}x {tenMon} cho bàn {chiTiet.HoaDon?.Ban?.TenBan}");

            // Lấy CuaHangId để gửi đúng group
            var storeId = chiTiet.HoaDon?.CuaHangId ?? 0;

            await _hubContext.Clients.Group($"store_{storeId}").SendAsync("MonAnDaXong", new
            {
                banId = chiTiet.HoaDon!.BanId,
                tenBan = chiTiet.HoaDon.Ban!.TenBan,
                tenMon = $"{chiTiet.SoLuong}x {tenMon} — Đã xong"
            });

            return Ok();
        }

        // ==========================================
        // 8. LẤY DANH SÁCH ĐƠN HÀNG (CHO ADMIN) - ĐÃ NÂNG CẤP
        // ==========================================
        [HttpGet("danh-sach-admin")]
        [Authorize(Roles = "SuperAdmin,ChuCuaHang,Admin,QuanLy,ThuNgan")]
        public async Task<IActionResult> GetDanhSachAdmin([FromQuery] int chiNhanhId, [FromQuery] string? search, [FromQuery] string? status, [FromQuery] string? startDate, [FromQuery] string? endDate)
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

                await _context.LogHoatDongAsync(chiNhanhId, "Danh sách đơn hàng", $"Xem danh sách đơn hàng. Tìm kiếm: '{search}', Trạng thái: '{status}', Từ ngày: '{startDate}', Đến ngày: '{endDate}'");
                var query = _context.HoaDons
                    .Include(h => h.Ban)
                    .Include(h => h.KhachHang)
                    .Include(h => h.ChiTietHoaDons!)
                    .ThenInclude(c => c.SanPham)
                    .Where(h => h.CuaHangId == cuaHangId);

                if (chiNhanhId > 0) query = query.Where(h => h.ChiNhanhId == chiNhanhId);

                if (!string.IsNullOrEmpty(status)) query = query.Where(h => h.TrangThai == status);

                if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var start))
                    query = query.Where(h => h.NgayTao >= start.Date);
                if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var end))
                    query = query.Where(h => h.NgayTao <= end.Date.AddDays(1).AddTicks(-1));

                var list = await query
                    .OrderByDescending(h => h.NgayTao)
                    .Select(h => new
                    {
                        Id = h.Id,
                        MaChungTu = $"HD{h.NgayTao:ddMMyy}-{h.Id:D4}",
                        TenBan = h.Ban != null ? h.Ban.TenBan : "Mang về",
                        KhachHang = h.KhachHang != null ? h.KhachHang.TenKhachHang : "Khách lẻ",
                        NgayBan = h.NgayTao,
                        TongCong = h.ChiTietHoaDons!.Any() ? h.ChiTietHoaDons!.Sum(ct => ct.SoLuong * ct.DonGia) : 0,
                        TongThanhToan = h.TongTien,
                        TienGiam = Math.Max(0, (h.ChiTietHoaDons!.Any() ? h.ChiTietHoaDons!.Sum(ct => ct.SoLuong * ct.DonGia) : 0) - h.TongTien),
                        TrangThai = h.TrangThai,
                        GhiChu = h.GhiChu,
                        ChiTiets = h.ChiTietHoaDons!.Select(ct => new
                        {
                            ChiTietId = ct.Id,
                            SanPhamId = ct.SanPhamId,
                            TenSanPham = ct.SanPham != null ? ct.SanPham.TenSanPham : "SP đã xóa",
                            SoLuong = ct.SoLuong,
                            DonGia = ct.DonGia,
                            ThanhTien = ct.SoLuong * ct.DonGia,
                            GhiChu = ct.GhiChu
                        }).ToList()
                    })
                    .ToListAsync();

                if (!string.IsNullOrEmpty(search))
                {
                    search = search.ToLower();
                    list = list.Where(x => x.MaChungTu.ToLower().Contains(search)).ToList();
                }

                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi server: " + ex.Message);
            }
        }

        public class HuyMonDto
        {
            public int ChiTietId { get; set; }
            public int SoLuongHuy { get; set; }
            public string LyDo { get; set; } = string.Empty;
            public string? Passcode { get; set; }
        }

        // ==========================================
        // 9. HỦY MÓN / TRẢ ĐỒ
        // ==========================================
        [HttpPost("huymon")]
        [Authorize(Roles = "SuperAdmin,ChuCuaHang,Admin,QuanLy,ThuNgan,Order")]
        public async Task<IActionResult> HuyMon([FromBody] HuyMonDto request)
        {
            // BUG-13 FIX: Ràng buộc số lượng hủy phải lớn hơn 0
            if (request.SoLuongHuy <= 0)
                return BadRequest("Số lượng hủy phải lớn hơn 0!");

            int cuaHangId = GetCuaHangId();
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? User.FindFirst("VaiTro")?.Value;

            // BUG-11/Perm check: Nhân viên Order hủy món
            if (role == "Order")
            {
                var isPermitted = await GetThietLapBoolAsync(cuaHangId, "Perm_Order_HuyMon", true);
                if (!isPermitted)
                    return StatusCode(403, "Nhân viên Order không được cấp quyền hủy món!");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var chiTiet = await _context.ChiTietHoaDons
                    .Include(c => c.SanPham) // Cần có sản phẩm để lấy tên ghi log
                    .Include(c => c.HoaDon)
                    .ThenInclude(h => h.Ban)
                    .FirstOrDefaultAsync(c => c.Id == request.ChiTietId);

                if (chiTiet == null) return BadRequest("Không tìm thấy món ăn này trong hóa đơn!");

                var hoaDon = chiTiet.HoaDon;

                // BUG-13 FIX: Kiểm tra phân quyền CuaHangId để tránh tenant leak
                if (hoaDon == null || hoaDon.CuaHangId != cuaHangId)
                    return StatusCode(403, "Bạn không có quyền thao tác trên hóa đơn này!");

                decimal tienBiTru = chiTiet.DonGia * request.SoLuongHuy;
                hoaDon!.TongTien -= tienBiTru;

                if (request.SoLuongHuy >= chiTiet.SoLuong)
                {
                    _context.ChiTietHoaDons.Remove(chiTiet);
                    await _context.SaveChangesAsync();

                    bool conMonNaoKhong = await _context.ChiTietHoaDons.AnyAsync(c => c.HoaDonId == hoaDon.Id);

                    if (!conMonNaoKhong)
                    {
                        // FIX-SEC-1: Thu ngân hủy hóa đơn - kiểm tra quyền VÀ kiểm tra bắt buộc PIN nếu cấu hình yêu cầu
                        if (role == "ThuNgan")
                        {
                            var isPermitted = await GetThietLapBoolAsync(cuaHangId, "Perm_ThuNgan_XoaHoaDon", true);
                            var requirePin = await GetThietLapBoolAsync(cuaHangId, "POS_YeuCauMatKhauHuyBill", false);

                            // Cần xác thực nếu: HOẶC không có quyền, HOẶC chủ quán bật yêu cầu PIN bắt buộc
                            bool needAuth = !isPermitted || requirePin;

                            if (needAuth)
                            {
                                if (string.IsNullOrEmpty(request.Passcode))
                                {
                                    string msg = !isPermitted
                                        ? "Thu ngân không được cấp quyền hủy/xóa hóa đơn! Vui lòng nhập mã PIN của Chủ cửa hàng."
                                        : "Cấu hình yêu cầu nhập mã PIN để xóa bill. Vui lòng nhập mã PIN của Chủ cửa hàng.";
                                    return StatusCode(403, msg);
                                }

                                // 1. Kiểm tra mã PIN nhanh (Security_AdminPIN)
                                var adminPin = await _context.ThietLaps
                                    .FirstOrDefaultAsync(t => t.CuaHangId == cuaHangId && t.MaThietLap == "Security_AdminPIN");
                                string pinCheck = adminPin?.DuLieu ?? "1234";

                                bool verified = (request.Passcode == pinCheck);

                                // 2. Nếu không khớp PIN nhanh, kiểm tra mật khẩu tài khoản chủ
                                if (!verified)
                                {
                                    var chuQuan = await _context.TaiKhoans
                                        .FirstOrDefaultAsync(t => t.CuaHangId == cuaHangId && t.VaiTro == "ChuCuaHang" && t.IsActive);
                                    if (chuQuan != null && BCrypt.Net.BCrypt.Verify(request.Passcode, chuQuan.MatKhauHash))
                                    {
                                        verified = true;
                                    }
                                }

                                if (!verified)
                                {
                                    return StatusCode(403, "Mật khẩu hoặc mã PIN xác thực của Chủ cửa hàng không chính xác!");
                                }
                            }
                        }

                        hoaDon.TrangThai = "Đã hủy";
                        if (hoaDon.Ban != null)
                        {
                            hoaDon.Ban.TrangThai = "Trống";
                        }
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    // FIX-SEC-2: Kiểm tra quyền 'Perm_ThuNgan_HuyMonDaGui' khi hủy 1 phần món đã gửi bếp
                    if (role == "ThuNgan" && chiTiet.TrangThaiMon == "Đã Gửi Bếp")
                    {
                        var canCancelSent = await GetThietLapBoolAsync(cuaHangId, "Perm_ThuNgan_HuyMonDaGui", false);
                        if (!canCancelSent)
                        {
                            // Yêu cầu xác thực PIN để hủy món đã gửi bếp
                            if (string.IsNullOrEmpty(request.Passcode))
                                return StatusCode(403, "Thu ngân không được cấp quyền hủy món đã gửi bếp! Vui lòng nhập mã PIN của Chủ cửa hàng.");

                            var adminPinSent = await _context.ThietLaps
                                .FirstOrDefaultAsync(t => t.CuaHangId == cuaHangId && t.MaThietLap == "Security_AdminPIN");
                            string pinCheckSent = adminPinSent?.DuLieu ?? "1234";

                            bool verifiedSent = (request.Passcode == pinCheckSent);
                            if (!verifiedSent)
                            {
                                var chuQuanSent = await _context.TaiKhoans
                                    .FirstOrDefaultAsync(t => t.CuaHangId == cuaHangId && t.VaiTro == "ChuCuaHang" && t.IsActive);
                                if (chuQuanSent != null && BCrypt.Net.BCrypt.Verify(request.Passcode, chuQuanSent.MatKhauHash))
                                    verifiedSent = true;
                            }

                            if (!verifiedSent)
                                return StatusCode(403, "Mã PIN xác thực không đúng! Không thể hủy món đã gửi bếp.");
                        }
                    }

                    chiTiet.SoLuong -= request.SoLuongHuy;
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                // Ghi nhận nhật ký hoạt động
                await _context.LogHoatDongAsync(hoaDon.ChiNhanhId, "Hủy món", $"Hủy {request.SoLuongHuy}x {chiTiet.SanPham?.TenSanPham} của bàn {hoaDon.Ban?.TenBan}. Lý do: {request.LyDo}");

                // Phát sự kiện SignalR để tất cả màn hình cập nhật realtime
                // Lấy CuaHangId từ hóa đơn
                int storeId = hoaDon?.CuaHangId ?? 0;
                if (_hubContext != null)
                {
                    await _hubContext.Clients.Group($"store_{storeId}").SendAsync("CapNhatBan", new { message = "Hủy món" });
                }

                return Ok(new { message = "Hủy món thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi hệ thống: " + ex.Message);
            }
        }

        [HttpPost("log-in-bill")]
        [Authorize(Roles = "ChuCuaHang,Admin,QuanLy,ThuNgan,Order")]
        public async Task<IActionResult> LogInBill([FromBody] LogInBillRequest request)
        {
            if (request == null) return BadRequest("Dữ liệu không hợp lệ.");
            
            int chiNhanhId = request.ChiNhanhId;
            if (chiNhanhId == 0 && request.BanId > 0)
            {
                var ban = await _context.Bans.Include(b => b.KhuVuc).FirstOrDefaultAsync(b => b.Id == request.BanId);
                chiNhanhId = ban?.KhuVuc?.ChiNhanhId ?? 0;
            }

            var branchClaim = User.FindFirst("ChiNhanhId");
            if (branchClaim != null)
            {
                int userBranchId = int.Parse(branchClaim.Value);
                if (chiNhanhId > 0 && chiNhanhId != userBranchId)
                {
                    return StatusCode(403, "Bạn không có quyền thực hiện nghiệp vụ cho chi nhánh khác!");
                }
                chiNhanhId = userBranchId;
            }

            string loaiIn = request.LoaiIn ?? "In hóa đơn";
            string target = !string.IsNullOrEmpty(request.TenBan) ? request.TenBan : (!string.IsNullOrEmpty(request.MaChungTu) ? $"mã HD {request.MaChungTu}" : "bàn chưa rõ");

            await _context.LogHoatDongAsync(chiNhanhId, "In hóa đơn", $"In {loaiIn} cho {target}. Tổng tiền: {request.TongTien:N0}đ");
            return Ok(new { success = true });
        }

        [HttpPost("hoan-tra")]
        [Authorize(Roles = "SuperAdmin,ChuCuaHang,Admin,QuanLy,ThuNgan")]
        public async Task<IActionResult> HoanTraHoaDon([FromBody] HoanTraDto request)
        {
            int cuaHangId = GetCuaHangId();
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? User.FindFirst("VaiTro")?.Value;

            // 1. Kiểm tra cấu hình hoàn trả có được bật không
            bool choPhepHoanTra = await GetThietLapBoolAsync(cuaHangId, "POS_ChoPhepHoanTraMon", true);
            if (!choPhepHoanTra)
            {
                return BadRequest("Chức năng hoàn trả món sau khi thanh toán chưa được kích hoạt!");
            }

            if (request.ChiTiets == null || !request.ChiTiets.Any())
            {
                return BadRequest("Danh sách món hoàn trả trống!");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 2. Tìm hóa đơn đã thanh toán
                var hoaDon = await _context.HoaDons
                    .Include(h => h.ChiTietHoaDons!)
                    .ThenInclude(ct => ct.SanPham)
                    .Include(h => h.KhachHang)
                    .Include(h => h.Ban)
                    .FirstOrDefaultAsync(h => h.Id == request.HoaDonId && h.CuaHangId == cuaHangId);

                if (hoaDon == null)
                {
                    return BadRequest("Không tìm thấy hóa đơn cần hoàn trả!");
                }

                if (hoaDon.TrangThai != "Đã thanh toán" && hoaDon.TrangThai != "Đã hoàn trả một phần")
                {
                    return BadRequest("Chỉ có thể hoàn trả món cho hóa đơn đã thanh toán!");
                }

                // 3. Kiểm tra bảo mật mã PIN nếu được yêu cầu (hoặc nếu là ThuNgan và cấu hình yêu cầu PIN khi hủy bill đang bật)
                var requirePin = await GetThietLapBoolAsync(cuaHangId, "POS_YeuCauMatKhauHuyBill", false);
                if (requirePin && role == "ThuNgan")
                {
                    if (string.IsNullOrEmpty(request.Passcode))
                    {
                        return StatusCode(403, "Yêu cầu nhập mã PIN của Chủ cửa hàng/Quản lý để thực hiện hoàn trả!");
                    }

                    var adminPin = await _context.ThietLaps
                        .FirstOrDefaultAsync(t => t.CuaHangId == cuaHangId && t.MaThietLap == "Security_AdminPIN");
                    string pinCheck = adminPin?.DuLieu ?? "1234";

                    bool verified = (request.Passcode == pinCheck);
                    if (!verified)
                    {
                        var chuQuan = await _context.TaiKhoans
                            .FirstOrDefaultAsync(t => t.CuaHangId == cuaHangId && t.VaiTro == "ChuCuaHang" && t.IsActive);
                        if (chuQuan != null && BCrypt.Net.BCrypt.Verify(request.Passcode, chuQuan.MatKhauHash))
                        {
                            verified = true;
                        }
                    }

                    if (!verified)
                    {
                        return StatusCode(403, "Mã PIN hoặc mật khẩu xác thực không đúng!");
                    }
                }

                // Tính toán tỷ lệ thanh toán ban đầu (nếu có chiết khấu hoặc dùng điểm)
                // Cần tính tổng cộng nguyên bản của các chi tiết hóa đơn (trước đợt trả này)
                decimal originalTongCong = hoaDon.ChiTietHoaDons!.Sum(ct => ct.SoLuong * ct.DonGia);
                if (originalTongCong <= 0)
                {
                    return BadRequest("Không thể hoàn trả hóa đơn có giá trị bằng 0!");
                }

                // Ti le thanh toan thuc te (sau khi tru chiet khau, dung diem, giam gia)
                decimal tiLeThanhToan = hoaDon.TongTien / originalTongCong;

                decimal tongTienHoanTraThucTe = 0;
                string detailsLog = "";

                foreach (var reqCt in request.ChiTiets)
                {
                    if (reqCt.SoLuongTra <= 0) continue;

                    var chiTiet = hoaDon.ChiTietHoaDons.FirstOrDefault(ct => ct.Id == reqCt.ChiTietId);
                    if (chiTiet == null)
                    {
                        return BadRequest($"Không tìm thấy món ăn có mã chi tiết {reqCt.ChiTietId} trong hóa đơn!");
                    }

                    if (reqCt.SoLuongTra > chiTiet.SoLuong)
                    {
                        return BadRequest($"Số lượng hoàn trả cho món '{chiTiet.SanPham?.TenSanPham}' vượt quá số lượng hiện tại ({chiTiet.SoLuong})!");
                    }

                    // Hoàn lại kho hàng
                    var tonKho = await _context.TonKhos
                        .FromSqlRaw("SELECT * FROM TonKhos WITH (UPDLOCK, ROWLOCK) WHERE SanPhamId = {0} AND ChiNhanhId = {1}", chiTiet.SanPhamId, hoaDon.ChiNhanhId)
                        .FirstOrDefaultAsync();

                    if (tonKho != null)
                    {
                        tonKho.SoLuong += reqCt.SoLuongTra;
                    }
                    else
                    {
                        _context.TonKhos.Add(new TonKho
                        {
                            SanPhamId = chiTiet.SanPhamId,
                            ChiNhanhId = hoaDon.ChiNhanhId,
                            SoLuong = reqCt.SoLuongTra
                        });
                    }

                    // Tính tiền món trả trước chiết khấu
                    decimal tienMonTra = chiTiet.DonGia * reqCt.SoLuongTra;
                    // Tính tiền thực tế hoàn trả (sau khi nhân tỷ lệ giảm giá/điểm)
                    decimal tienHoanTraMonThucTe = tienMonTra * tiLeThanhToan;
                    tongTienHoanTraThucTe += tienHoanTraMonThucTe;

                    // Giảm số lượng trên hóa đơn
                    chiTiet.SoLuong -= reqCt.SoLuongTra;

                    detailsLog += $"{reqCt.SoLuongTra}x {chiTiet.SanPham?.TenSanPham}, ";
                }

                if (tongTienHoanTraThucTe <= 0)
                {
                    return BadRequest("Không có món nào được hoàn trả!");
                }

                // Cập nhật lại tổng tiền hóa đơn sau hoàn trả
                decimal newTongTien = hoaDon.TongTien - tongTienHoanTraThucTe;
                if (newTongTien < 0) newTongTien = 0;
                hoaDon.TongTien = newTongTien;

                // Xử lý điểm tích lũy của khách hàng (nếu có)
                int diemKhauTru = 0;
                if (hoaDon.KhachHangId.HasValue && hoaDon.KhachHang != null)
                {
                    decimal tyLeTichDiem = 20000m;
                    var tlTichDiem = await _context.ThietLaps
                        .FirstOrDefaultAsync(t => t.CuaHangId == cuaHangId && t.MaThietLap == "Loyalty_TiLeKiem");
                    if (tlTichDiem != null && decimal.TryParse(tlTichDiem.DuLieu, out decimal parsedTichDiem) && parsedTichDiem > 0)
                    {
                        tyLeTichDiem = parsedTichDiem;
                    }

                    // Điểm cộng ban đầu trên phần tiền trả lại
                    diemKhauTru = (int)(tongTienHoanTraThucTe / tyLeTichDiem);
                    if (diemKhauTru > 0)
                    {
                        hoaDon.KhachHang.DiemHienTai -= diemKhauTru;
                        if (hoaDon.KhachHang.DiemHienTai < 0) hoaDon.KhachHang.DiemHienTai = 0;

                        hoaDon.KhachHang.TongDiemTichLuy -= diemKhauTru;
                        if (hoaDon.KhachHang.TongDiemTichLuy < 0) hoaDon.KhachHang.TongDiemTichLuy = 0;
                    }
                }

                // Tạo phiếu chi hoàn trả tiền cho khách hàng
                var phieuChi = new PhieuThuChi
                {
                    CuaHangId = hoaDon.CuaHangId,
                    ChiNhanhId = hoaDon.ChiNhanhId,
                    MaChungTu = $"PC{DateTime.Now:ddMMyy}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
                    LoaiPhieu = "Chi",
                    PhuongThuc = hoaDon.PhuongThucThanhToan,
                    NguoiNopNhan = hoaDon.KhachHang != null ? hoaDon.KhachHang.TenKhachHang : "Khách lẻ",
                    HangMuc = "Hoàn trả hàng",
                    LyDo = $"Hoàn tiền khách trả món từ HD {hoaDon.Id} ({request.LyDo})",
                    GiaTri = (double)tongTienHoanTraThucTe,
                    NgayGiaoDich = DateTime.Now,
                    NguoiTao = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value ?? "Thu ngân"
                };
                _context.PhieuThuChis.Add(phieuChi);

                // Cập nhật trạng thái hóa đơn
                bool conMonNaoKhong = hoaDon.ChiTietHoaDons.Any(ct => ct.SoLuong > 0);
                if (!conMonNaoKhong)
                {
                    hoaDon.TrangThai = "Đã trả hàng"; // Trả hàng toàn bộ
                }
                else
                {
                    hoaDon.TrangThai = "Đã hoàn trả một phần"; // Trả hàng một phần
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Ghi nhật ký hoạt động
                await _context.LogHoatDongAsync(hoaDon.ChiNhanhId, "Hoàn trả hàng", $"Hoàn trả món hóa đơn HD {hoaDon.Id}. Danh sách trả: {detailsLog.TrimEnd(',', ' ')}. Lý do: {request.LyDo}. Số tiền hoàn: {tongTienHoanTraThucTe:N0}đ");

                if (_hubContext != null)
                {
                    await _hubContext.Clients.Group($"store_{cuaHangId}").SendAsync("CapNhatBan", new { message = "Đã hoàn trả món" });
                }

                return Ok(new { message = "Hoàn trả món thành công!", tongTienHoanTra = tongTienHoanTraThucTe, diemKhauTru });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi server: " + ex.Message);
            }
        }

        [HttpPost("capnhat-ghichu/{banId}")]
        [Authorize(Roles = "SuperAdmin,ChuCuaHang,Admin,QuanLy,ThuNgan,Order")]
        public async Task<IActionResult> CapNhatGhiChu(int banId, [FromBody] CapNhatGhiChuDto request)
        {
            int cuaHangId = GetCuaHangId();
            var hoaDon = await _context.HoaDons
                .FirstOrDefaultAsync(h => h.BanId == banId && h.TrangThai == "Đang phục vụ" && h.CuaHangId == cuaHangId);

            if (hoaDon == null) return NotFound("Bàn này chưa có hóa đơn mở!");

            hoaDon.GhiChu = request.GhiChu ?? "";
            await _context.SaveChangesAsync();

            if (_hubContext != null)
            {
                await _hubContext.Clients.Group($"store_{cuaHangId}").SendAsync("CapNhatBan", new { message = "Cập nhật ghi chú hóa đơn" });
            }

            return Ok(new { message = "Đã cập nhật ghi chú hóa đơn thành công!" });
        }

        private async Task<bool> GetThietLapBoolAsync(int cuaHangId, string key, bool defaultValue)
        {
            var tl = await _context.ThietLaps
                .FirstOrDefaultAsync(t => t.CuaHangId == cuaHangId && t.MaThietLap == key);
            if (tl == null || string.IsNullOrEmpty(tl.DuLieu)) return defaultValue;
            return tl.DuLieu.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class CapNhatGhiChuDto
    {
        public string GhiChu { get; set; } = string.Empty;
    }

    public class LogInBillRequest
    {
        public int BanId { get; set; }
        public int ChiNhanhId { get; set; }
        public string? TenBan { get; set; }
        public string? MaChungTu { get; set; }
        public string? LoaiIn { get; set; }
        public decimal TongTien { get; set; }
    }

    public class HoanTraDto
    {
        public int HoaDonId { get; set; }
        public List<HoanTraChiTietDto> ChiTiets { get; set; } = new();
        public string LyDo { get; set; } = "";
        public string? Passcode { get; set; }
    }

    public class HoanTraChiTietDto
    {
        public int ChiTietId { get; set; }
        public int SoLuongTra { get; set; }
    }
}