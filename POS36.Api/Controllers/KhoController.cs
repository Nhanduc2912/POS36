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

        public KhoController(AppDbContext context)
        {
            _context = context;
        }

        private int GetTaiKhoanId()
        {
            var claim = User.FindFirst("Id"); // Lấy ID của người đang đăng nhập
            if (claim == null) throw new UnauthorizedAccessException("Token không hợp lệ");
            return int.Parse(claim.Value);
        }

        // ==========================================
        // 1. TẠO PHIẾU NHẬP KHO
        // ==========================================
        [HttpPost("nhapkho")]
        public async Task<IActionResult> NhapKho(TaoPhieuNhapDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Tạo Phiếu Nhập
                var phieuNhap = new PhieuNhap
                {
                    ChiNhanhId = request.ChiNhanhId,
                    TaiKhoanId = GetTaiKhoanId(),
                    NgayNhap = DateTime.Now,
                    GhiChu = request.GhiChu
                };

                _context.PhieuNhaps.Add(phieuNhap);
                await _context.SaveChangesAsync(); // Lưu để lấy ID Phiếu

                // 2. Duyệt qua từng món hàng để Lưu chi tiết, Cộng Tồn kho & Ghi Lịch sử
                foreach (var item in request.DanhSachNhap)
                {
                    // Lưu chi tiết
                    var chiTiet = new ChiTietPhieuNhap
                    {
                        PhieuNhapId = phieuNhap.Id,
                        SanPhamId = item.SanPhamId,
                        SoLuong = item.SoLuong,
                        DonGiaNhap = item.DonGiaNhap
                    };
                    _context.ChiTietPhieuNhaps.Add(chiTiet);

                    // Cập nhật Tồn Kho (Nếu có rồi thì cộng dồn, chưa có thì tạo dòng mới)
                    var tonKho = await _context.TonKhos
                        .FirstOrDefaultAsync(t => t.ChiNhanhId == request.ChiNhanhId && t.SanPhamId == item.SanPhamId);

                    if (tonKho != null)
                    {
                        tonKho.SoLuong += item.SoLuong;
                    }
                    else
                    {
                        tonKho = new TonKho
                        {
                            ChiNhanhId = request.ChiNhanhId,
                            SanPhamId = item.SanPhamId,
                            SoLuong = item.SoLuong
                        };
                        _context.TonKhos.Add(tonKho);
                    }

                    // Ghi Lịch Sử Kho (Thẻ kho để truy vết)
                    var lichSu = new LichSuKho
                    {
                        ChiNhanhId = request.ChiNhanhId,
                        SanPhamId = item.SanPhamId,
                        LoaiGiaoDich = "Nhập kho",
                        SoLuong = item.SoLuong,
                        NgayThucHien = DateTime.Now,
                        GhiChu = $"Nhập theo Phiếu #{phieuNhap.Id}"
                    };
                    _context.LichSuKhos.Add(lichSu);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Nhập kho thành công!", phieuNhapId = phieuNhap.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Lỗi hệ thống: {ex.Message}");
            }
        }

        // ==========================================
        // 2. XEM SỐ LƯỢNG TỒN KHO HIỆN TẠI
        // ==========================================
        [HttpGet("tonkho/{chiNhanhId}")]
        public async Task<IActionResult> GetTonKho(int chiNhanhId)
        {
            var tonKho = await _context.TonKhos
                .Include(t => t.SanPham)
                .Where(t => t.ChiNhanhId == chiNhanhId)
                .Select(t => new
                {
                    t.SanPhamId,
                    TenSanPham = t.SanPham!.TenSanPham,
                    t.SoLuong
                }).ToListAsync();

            return Ok(tonKho);
        }
    }
}