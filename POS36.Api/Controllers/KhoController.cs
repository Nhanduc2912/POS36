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
    public class KhoController : ControllerBase
    {
        private readonly AppDbContext _context;
        public KhoController(AppDbContext context) { _context = context; }

        [HttpPost("nhap-hang")]
        public async Task<IActionResult> NhapHang([FromBody] TaoPhieuNhapDto request)
        {
            // 1. Kiểm tra quyền sở hữu Chi Nhánh
            var cuaHangId = int.Parse(User.FindFirst("CuaHangId")!.Value);
            var validBranch = await _context.ChiNhanhs.AnyAsync(c => c.Id == request.ChiNhanhId && c.CuaHangId == cuaHangId);
            if (!validBranch) return BadRequest("Chi nhánh không hợp lệ!");

            if (request.ChiTiets == null || !request.ChiTiets.Any())
                return BadRequest("Phiếu nhập phải có ít nhất 1 mặt hàng!");

            // Lấy ID Tài khoản đang thao tác (Tạm thời fix cứng hoặc lấy từ Token nếu em có)
            var taiKhoanId = int.Parse(User.FindFirst("Id")?.Value ?? "1");

            // BẮT ĐẦU TRANSACTION ĐỂ ĐẢM BẢO AN TOÀN DỮ LIỆU
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 2. Tạo Phiếu Nhập
                var phieuNhap = new PhieuNhap
                {
                    ChiNhanhId = request.ChiNhanhId,
                    TaiKhoanId = taiKhoanId, // Ai là người nhập
                    NgayNhap = DateTime.Now,
                    GhiChu = request.GhiChu
                };
                _context.PhieuNhaps.Add(phieuNhap);
                await _context.SaveChangesAsync(); // Lưu để lấy ID Phiếu Nhập

                // 3. Xử lý từng mặt hàng: Lưu Chi Tiết + Cộng Tồn Kho
                foreach (var item in request.ChiTiets)
                {
                    // Thêm Chi tiết phiếu nhập
                    var chiTiet = new ChiTietPhieuNhap
                    {
                        PhieuNhapId = phieuNhap.Id,
                        SanPhamId = item.SanPhamId,
                        SoLuong = item.SoLuong,
                        DonGiaNhap = item.GiaNhap
                    };
                    _context.ChiTietPhieuNhaps.Add(chiTiet);

                    // XỬ LÝ TỒN KHO RIÊNG BIỆT CHO CHI NHÁNH NÀY
                    var tonKho = await _context.TonKhos
                        .FirstOrDefaultAsync(t => t.SanPhamId == item.SanPhamId && t.ChiNhanhId == request.ChiNhanhId);

                    if (tonKho != null)
                    {
                        // Đã có trong kho chi nhánh này -> Cộng dồn
                        tonKho.SoLuong += item.SoLuong;
                    }
                    else
                    {
                        // Chưa từng có trong kho chi nhánh này -> Tạo mới
                        var newTonKho = new TonKho
                        {
                            ChiNhanhId = request.ChiNhanhId,
                            SanPhamId = item.SanPhamId,
                            SoLuong = item.SoLuong
                        };
                        _context.TonKhos.Add(newTonKho);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync(); // Xác nhận thành công toàn bộ

                return Ok(new { message = "Nhập hàng thành công!", phieuNhapId = phieuNhap.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Nếu lỗi ở bất kỳ bước nào, hoàn tác toàn bộ!
                return StatusCode(500, new { message = "Lỗi xử lý nhập hàng", error = ex.Message });
            }
        }
    }
}