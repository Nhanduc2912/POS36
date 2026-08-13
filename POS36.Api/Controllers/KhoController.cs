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
                        NguyenVatLieuId = item.NguyenVatLieuId,
                        SoLuong = item.SoLuong,
                        DonGiaNhap = item.DonGiaNhap,
                        NgayHetHan = item.NgayHetHan
                    };
                    _context.ChiTietPhieuNhaps.Add(chiTiet);

                    // =====================================================
                    // TÍNH GIÁ VỐN BÌNH QUÂN GIA QUYỀN (Moving Average Cost)
                    // Công thức: MAC = (Tồn cũ × Giá vốn cũ + SL nhập × Giá nhập) / (Tồn cũ + SL nhập)
                    // =====================================================
                    var nvl = await _context.NguyenVatLieus.FindAsync(item.NguyenVatLieuId);
                    if (nvl != null)
                    {
                        // Lấy tổng tồn kho hiện tại TRƯỚC khi cộng thêm (tất cả chi nhánh)
                        decimal tonKhoCu = await _context.TonKhos
                            .Where(t => t.NguyenVatLieuId == item.NguyenVatLieuId)
                            .SumAsync(t => (decimal?)t.SoLuong) ?? 0;

                        decimal giaVonCu = nvl.GiaVonHienTai;
                        decimal slNhapMoi = item.SoLuong;
                        decimal giaNhapMoi = item.DonGiaNhap;

                        // Áp dụng công thức MAC
                        decimal tongGiaTri = (tonKhoCu * giaVonCu) + (slNhapMoi * giaNhapMoi);
                        decimal tongSoLuong = tonKhoCu + slNhapMoi;

                        nvl.GiaVonHienTai = tongSoLuong > 0
                            ? Math.Round(tongGiaTri / tongSoLuong, 2)
                            : giaNhapMoi; // Nếu tồn = 0 thì lấy giá nhập mới
                    }

                    // XỬ LÝ TỒN KHO RIÊNG BIỆT CHO CHI NHÁNH VÀ NGÀY HẾT HẠN (LÔ)
                    var tonKho = await _context.TonKhos
                        .FirstOrDefaultAsync(t => t.NguyenVatLieuId == item.NguyenVatLieuId && t.ChiNhanhId == request.ChiNhanhId && t.NgayHetHan == item.NgayHetHan);

                    if (tonKho != null)
                    {
                        // Đã có trong kho lô này -> Cộng dồn
                        tonKho.SoLuong += item.SoLuong;
                    }
                    else
                    {
                        // Chưa từng có lô này -> Tạo mới
                        var newTonKho = new TonKho
                        {
                            ChiNhanhId = request.ChiNhanhId,
                            NguyenVatLieuId = item.NguyenVatLieuId,
                            SoLuong = item.SoLuong,
                            NgayHetHan = item.NgayHetHan
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