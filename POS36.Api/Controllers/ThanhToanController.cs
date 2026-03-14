using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using POS36.Api.DTOs;
using POS36.Api.Models;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ThanhToanController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ThanhToanController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCuaHangId()
        {
            var claim = User.FindFirst("CuaHangId");
            if (claim == null) throw new UnauthorizedAccessException("Token không hợp lệ");
            return int.Parse(claim.Value);
        }

        [HttpPost("chot-bill")]
        public async Task<IActionResult> ChotBill(TaoThanhToanDto request)
        {
            int cuaHangId = GetCuaHangId();

            // Dùng Transaction để đảm bảo tính toàn vẹn dữ liệu
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Tìm Hóa Đơn cần thanh toán (Bắt buộc phải thuộc Cửa hàng này và đang mở)
                var hoaDon = await _context.HoaDons
                    .Include(h => h.ChiTietHoaDons) // Kéo theo chi tiết để tính lại tiền cho chắc chắn
                    .Include(h => h.Ban) // Kéo theo Bàn để tí nữa update trạng thái
                    .FirstOrDefaultAsync(h => h.Id == request.HoaDonId && h.CuaHangId == cuaHangId && h.TrangThai == "Đang phục vụ");

                if (hoaDon == null) return BadRequest("Hóa đơn không tồn tại hoặc đã được thanh toán!");

                // Tính toán lại Tổng tiền từ DB để chống Hacker sửa giá từ Frontend truyền lên
                decimal tongTienThucTe = hoaDon.ChiTietHoaDons!.Sum(ct => ct.DonGia * ct.SoLuong);

                if (request.SoTienKhachDua < tongTienThucTe)
                    return BadRequest($"Khách đưa thiếu tiền! Tổng bill là: {tongTienThucTe:N0}đ");

                // 2. Tạo bản ghi Thanh Toán
                var thanhToan = new ThanhToan
                {
                    HoaDonId = hoaDon.Id,
                    PhuongThuc = request.PhuongThuc,
                    SoTien = tongTienThucTe, // Chỉ ghi nhận doanh thu bằng đúng số tiền Bill
                    NgayThanhToan = DateTime.Now
                };
                _context.ThanhToans.Add(thanhToan);

                // 3. Cập nhật trạng thái Hóa Đơn
                hoaDon.TrangThai = "Đã thanh toán";
                hoaDon.NgayThanhToan = DateTime.Now;
                hoaDon.TongTien = tongTienThucTe; // Chốt hạ con số cuối cùng

                // 4. Giải phóng Bàn về trạng thái "Trống"
                if (hoaDon.Ban != null)
                {
                    hoaDon.Ban.TrangThai = "Trống";
                }

                // 5. Lưu xuống Database và Commit
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                decimal tienThoiLai = request.SoTienKhachDua - tongTienThucTe;

                return Ok(new
                {
                    message = "Thanh toán thành công!",
                    tongTien = tongTienThucTe,
                    tienThoi = tienThoiLai
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Lỗi hệ thống khi thanh toán: {ex.Message}");
            }
        }
    }
}