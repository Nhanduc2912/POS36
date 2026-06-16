using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using POS36.Api.Models;
using POS36.Api.DTOs; // Bắt buộc phải có để gọi được TaoPhieuKiemKeDto
using System;
using System.Linq;
using System.Threading.Tasks;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Bảo vệ API, bắt buộc phải có Token đăng nhập
    public class KiemKeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public KiemKeController(AppDbContext context)
        {
            _context = context;
        }

        // BUG #8 FIX: Không fallback về 1 — throw exception nếu token không hợp lệ
        private int GetCuaHangId()
        {
            var claim = User.FindFirst("CuaHangId")?.Value;
            if (claim == null) throw new UnauthorizedAccessException("Token không hợp lệ");
            return int.Parse(claim);
        }

        // ==========================================
        // 1. LẤY DANH SÁCH PHIẾU KIỂM KÊ (Màn hình 1)
        // ==========================================
        [HttpGet("danh-sach")]
        public async Task<IActionResult> GetDanhSach([FromQuery] int chiNhanhId)
        {
            try
            {
                int cuaHangId = GetCuaHangId();
                var query = _context.PhieuKiemKes.Where(p => p.CuaHangId == cuaHangId);

                // Lọc theo chi nhánh nếu có truyền lên
                if (chiNhanhId > 0)
                {
                    query = query.Where(p => p.ChiNhanhId == chiNhanhId);
                }

                var list = await query
                    .OrderByDescending(p => p.NgayTao)
                    .Select(p => new
                    {
                        p.Id,
                        p.MaChungTu,
                        p.NgayTao,
                        p.GhiChu,
                        p.TrangThai
                    }).ToListAsync();

                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi server: " + ex.Message);
            }
        }

        // ==========================================
        // 2. LẤY SP & TỒN KHO ĐỂ TÌM KIẾM (Màn hình Thêm Mới)
        // ==========================================
        [HttpGet("san-pham-ton-kho")]
        public async Task<IActionResult> GetSanPhamTonKho([FromQuery] int chiNhanhId)
        {
            try
            {
                int cuaHangId = GetCuaHangId();
                var list = await _context.SanPhams
                    .Include(s => s.DanhMuc) // Kèm Danh mục để lấy tên nhóm
                    .Where(s => s.CuaHangId == cuaHangId && s.TrangThai == true)
                    .Select(s => new
                    {
                        s.Id,
                        MaSanPham = "SP" + s.Id,
                        s.TenSanPham,
                        TenDanhMuc = s.DanhMuc != null ? s.DanhMuc.TenDanhMuc : "Khác", // Lấy Tên nhóm
                        TonKho = _context.TonKhos
                            .Where(t => t.SanPhamId == s.Id && t.ChiNhanhId == chiNhanhId)
                            .Select(t => t.SoLuong)
                            .FirstOrDefault()
                    }).ToListAsync();

                return Ok(list);
            }
            catch (Exception ex) { return StatusCode(500, "Lỗi server: " + ex.Message); }
        }
        // ==========================================
        // 3. TẠO PHIẾU KIỂM KÊ & CÂN BẰNG KHO (Màn hình Thêm Mới)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> TaoPhieuKiemKe([FromBody] TaoPhieuKiemKeDto request)
        {
            int cuaHangId = GetCuaHangId();

            // 1. Xác thực chi nhánh có thuộc cửa hàng hiện tại hay không để chống IDOR
            var checkChiNhanh = await _context.ChiNhanhs.AnyAsync(cn => cn.Id == request.ChiNhanhId && cn.CuaHangId == cuaHangId);
            if (!checkChiNhanh) return BadRequest("Chi nhánh không hợp lệ hoặc không thuộc cửa hàng của bạn!");

            // 2. Xác thực tất cả sản phẩm kiểm kê có thuộc cửa hàng hiện tại hay không
            foreach (var item in request.ChiTiets)
            {
                var checkSanPham = await _context.SanPhams.AnyAsync(sp => sp.Id == item.SanPhamId && sp.CuaHangId == cuaHangId);
                if (!checkSanPham) return BadRequest($"Sản phẩm (ID: {item.SanPhamId}) không hợp lệ hoặc không thuộc cửa hàng của bạn!");
            }

            // Dùng Transaction để đảm bảo: Nếu lỗi giữa chừng thì hủy toàn bộ, không lưu rác vào Database
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Tạo Phiếu Kiểm Kê
                var phieu = new PhieuKiemKe
                {
                    CuaHangId = cuaHangId,
                    ChiNhanhId = request.ChiNhanhId,
                    NgayTao = DateTime.Now,
                    TrangThai = request.TrangThai,
                    GhiChu = request.GhiChu,
                    // Tự động sinh mã (VD: KK260326-1234)
                    MaChungTu = $"KK{DateTime.Now:ddMMyy}-{new Random().Next(1000, 9999)}"
                };

                _context.PhieuKiemKes.Add(phieu);
                await _context.SaveChangesAsync(); // Cần lưu trước để SQL cấp cho cái phieu.Id

                // 2. Lưu Chi tiết mặt hàng & Cập nhật Tồn kho
                foreach (var item in request.ChiTiets)
                {
                    // Thêm vào bảng ChiTietKiemKe
                    _context.ChiTietKiemKes.Add(new ChiTietKiemKe
                    {
                        PhieuKiemKeId = phieu.Id,
                        SanPhamId = item.SanPhamId,
                        TonKhoHienTai = item.TonKhoHienTai,
                        SoLuongKiemKe = item.SoLuongKiemKe
                    });

                    // CHỐT SỔ: NẾU TRẠNG THÁI LÀ "Hoàn thành" -> TIẾN HÀNH CÂN BẰNG KHO
                    if (request.TrangThai == "Hoàn thành")
                    {
                        var tonKho = await _context.TonKhos
                            .FirstOrDefaultAsync(t => t.SanPhamId == item.SanPhamId && t.ChiNhanhId == request.ChiNhanhId);

                        if (tonKho == null)
                        {
                            // ĐÃ XÓA CuaHangId Ở ĐÂY CHO ĐÚNG THIẾT KẾ DATABASE
                            _context.TonKhos.Add(new TonKho
                            {
                                SanPhamId = item.SanPhamId,
                                ChiNhanhId = request.ChiNhanhId,
                                SoLuong = item.SoLuongKiemKe
                            });
                        }
                        else
                        {
                            // Ghi đè Tồn kho hệ thống = Số lượng nhân viên vừa đếm được!
                            tonKho.SoLuong = item.SoLuongKiemKe;
                        }
                    }
                }

                // Lưu tất cả thay đổi và xác nhận Transaction
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _context.LogHoatDongAsync(phieu.ChiNhanhId, "Kiểm kê kho", $"Tạo phiếu kiểm kê {phieu.MaChungTu} ({phieu.TrangThai}). Ghi chú: {phieu.GhiChu}");

                return Ok(new { message = "Lưu phiếu kiểm kê thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi khi lưu phiếu: " + ex.Message);
            }
        }
        // ==========================================
        // 4. LẤY CHI TIẾT 1 PHIẾU KIỂM KÊ
        // ==========================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetChiTiet(int id)
        {
            var phieu = await _context.PhieuKiemKes
                .Include(p => p.ChiTiets)
                    .ThenInclude(c => c.SanPham)
                .Where(p => p.Id == id && p.CuaHangId == GetCuaHangId())
                .Select(p => new
                {
                    p.Id,
                    p.MaChungTu,
                    p.NgayTao,
                    p.GhiChu,
                    p.TrangThai,
                    ChiTiets = p.ChiTiets.Select(c => new
                    {
                        c.SanPhamId,
                        MaSanPham = "SP" + c.SanPhamId,
                        TenSanPham = c.SanPham!.TenSanPham,
                        c.TonKhoHienTai,
                        c.SoLuongKiemKe
                    })
                })
                .FirstOrDefaultAsync();

            if (phieu == null) return NotFound("Không tìm thấy phiếu!");
            return Ok(phieu);
        }

        // ==========================================
        // 5. CẬP NHẬT PHIẾU KIỂM KÊ (PUT api/KiemKe/{id})
        // ==========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> CapNhatPhieuKiemKe(int id, [FromBody] TaoPhieuKiemKeDto request)
        {
            int cuaHangId = GetCuaHangId();

            // Xác thực tất cả sản phẩm kiểm kê có thuộc cửa hàng hiện tại hay không để chống IDOR
            foreach (var item in request.ChiTiets)
            {
                var checkSanPham = await _context.SanPhams.AnyAsync(sp => sp.Id == item.SanPhamId && sp.CuaHangId == cuaHangId);
                if (!checkSanPham) return BadRequest($"Sản phẩm (ID: {item.SanPhamId}) không hợp lệ hoặc không thuộc cửa hàng của bạn!");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var phieu = await _context.PhieuKiemKes
                    .Include(p => p.ChiTiets)
                    .FirstOrDefaultAsync(p => p.Id == id && p.CuaHangId == cuaHangId);

                if (phieu == null) return NotFound("Không tìm thấy phiếu kiểm kê!");

                if (phieu.TrangThai == "Hoàn thành")
                    return BadRequest("Phiếu kiểm kê này đã hoàn thành, không thể sửa đổi!");

                // Cập nhật thông tin chung
                phieu.GhiChu = request.GhiChu;
                phieu.TrangThai = request.TrangThai;

                // Xóa chi tiết cũ
                _context.ChiTietKiemKes.RemoveRange(phieu.ChiTiets);

                // Thêm chi tiết mới và cân bằng kho nếu Hoàn thành
                foreach (var item in request.ChiTiets)
                {
                    _context.ChiTietKiemKes.Add(new ChiTietKiemKe
                    {
                        PhieuKiemKeId = phieu.Id,
                        SanPhamId = item.SanPhamId,
                        TonKhoHienTai = item.TonKhoHienTai,
                        SoLuongKiemKe = item.SoLuongKiemKe
                    });

                    if (request.TrangThai == "Hoàn thành")
                    {
                        var tonKho = await _context.TonKhos
                            .FirstOrDefaultAsync(t => t.SanPhamId == item.SanPhamId && t.ChiNhanhId == phieu.ChiNhanhId);

                        if (tonKho == null)
                        {
                            _context.TonKhos.Add(new TonKho
                            {
                                SanPhamId = item.SanPhamId,
                                ChiNhanhId = phieu.ChiNhanhId,
                                SoLuong = item.SoLuongKiemKe
                            });
                        }
                        else
                        {
                            tonKho.SoLuong = item.SoLuongKiemKe;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _context.LogHoatDongAsync(phieu.ChiNhanhId, "Kiểm kê kho", $"Cập nhật phiếu kiểm kê {phieu.MaChungTu} ({phieu.TrangThai}). Ghi chú: {phieu.GhiChu}");

                return Ok(new { message = "Cập nhật phiếu kiểm kê thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi khi cập nhật phiếu: " + ex.Message);
            }
        }
    }
}