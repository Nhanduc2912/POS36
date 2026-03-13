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

            // 1. Kiểm tra Bàn có hợp lệ không
            var ban = await _context.Bans.FirstOrDefaultAsync(b => b.Id == request.BanId && b.CuaHangId == cuaHangId);
            if (ban == null) return BadRequest("Bàn không tồn tại!");

            // Bắt đầu Transaction (Đảm bảo lưu thành công tất cả hoặc không lưu gì cả)
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 2. Tìm Hóa đơn đang mở của bàn này, nếu chưa có thì tạo mới
                var hoaDon = await _context.HoaDons
                    .FirstOrDefaultAsync(h => h.BanId == request.BanId && h.TrangThai == "Đang phục vụ" && h.CuaHangId == cuaHangId);

                if (hoaDon == null)
                {
                    // Tạo hóa đơn mới
                    hoaDon = new HoaDon
                    {
                        CuaHangId = cuaHangId,
                        ChiNhanhId = ban.KhuVuc!.ChiNhanhId, // Lấy chi nhánh từ Khu vực của Bàn
                        BanId = request.BanId,
                        NgayTao = DateTime.Now,
                        TrangThai = "Đang phục vụ",
                        TongTien = 0
                    };
                    _context.HoaDons.Add(hoaDon);
                    await _context.SaveChangesAsync(); // Lưu để lấy HoaDonId

                    // Cập nhật trạng thái Bàn
                    ban.TrangThai = "Đang phục vụ";
                }


                decimal tongTienCongThem = 0;

                // 3. Duyệt qua từng món khách gọi để lưu vào Chi Tiết Hóa Đơn
                foreach (var mon in request.DanhSachMon)
                {
                    // Lấy giá bán thực tế của món ăn từ Database (Chống việc Frontend gửi giá ảo lên hack)
                    var sanPham = await _context.SanPhams.FindAsync(mon.SanPhamId);
                    if (sanPham == null || sanPham.CuaHangId != cuaHangId)
                        throw new Exception($"Sản phẩm ID {mon.SanPhamId} không tồn tại!");

                    var chiTiet = new ChiTietHoaDon
                    {
                        HoaDonId = hoaDon.Id,
                        SanPhamId = mon.SanPhamId,
                        SoLuong = mon.SoLuong,
                        DonGia = sanPham.GiaBan, // Lấy giá gốc trong DB
                        GhiChu = mon.GhiChu,
                        TrangThaiMon = "Chờ chế biến" // Tí nữa Bếp sẽ nhìn thấy trạng thái này
                    };

                    _context.ChiTietHoaDons.Add(chiTiet);

                    // Cộng dồn tiền
                    tongTienCongThem += (chiTiet.SoLuong * chiTiet.DonGia);
                }

                // 4. Cập nhật lại Tổng tiền của Hóa đơn
                hoaDon.TongTien += tongTienCongThem;

                // 5. Lưu toàn bộ thay đổi và Xác nhận Transaction
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // BẮN TÍN HIỆU REAL-TIME XUỐNG BẾP
                // Lệnh này gửi một sự kiện tên là "CoDonHangMoi" tới tất cả những ai đang cắm mạng vào KitchenHub
                await _hubContext.Clients.All.SendAsync("CoDonHangMoi", new
                {
                    banId = request.BanId,
                    tenBan = ban.TenBan,
                    hoaDonId = hoaDon.Id,
                    thoiGian = DateTime.Now.ToString("HH:mm:ss")
                });

                return Ok(new { message = "Gọi món thành công! Đã gửi xuống bếp.", hoaDonId = hoaDon.Id });
            }
            catch (Exception ex)
            {
                // Nếu có bất kỳ lỗi gì xảy ra (ví dụ mã món ăn sai), quay xe (Rollback) toàn bộ!
                await transaction.RollbackAsync();
                return StatusCode(500, $"Lỗi hệ thống: {ex.Message}");
            }
        }
    }
}