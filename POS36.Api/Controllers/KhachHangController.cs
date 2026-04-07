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
    public class KhachHangController : ControllerBase
    {
        private readonly AppDbContext _context;
        public KhachHangController(AppDbContext context) { _context = context; }

        private int GetCuaHangId() => int.Parse(User.FindFirst("CuaHangId")!.Value);

        // ==========================================
        // 1. LẤY DANH SÁCH KHÁCH HÀNG (CÓ TÌM KIẾM)
        // ==========================================
        [HttpGet("danh-sach")]
        public async Task<IActionResult> GetDanhSach([FromQuery] string? search)
        {
            int cuaHangId = GetCuaHangId();
            var query = _context.KhachHangs.Where(k => k.CuaHangId == cuaHangId);

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(k =>
                    k.TenKhachHang.ToLower().Contains(search) ||
                    k.SoDienThoai.Contains(search));
            }

            var list = await query
                .OrderByDescending(k => k.NgayTao)
                .Select(k => new
                {
                    k.Id,
                    k.TenKhachHang,
                    k.SoDienThoai,
                    k.Email,
                    k.DiaChi,
                    k.NgaySinh,
                    k.GhiChu,
                    k.TongDiemTichLuy,
                    k.DiemHienTai,
                    k.NgayTao
                })
                .ToListAsync();

            return Ok(list);
        }

        // ==========================================
        // 2. XEM CHI TIẾT KHÁCH HÀNG + LỊCH SỬ MUA
        // ==========================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetChiTiet(int id)
        {
            int cuaHangId = GetCuaHangId();
            var khach = await _context.KhachHangs
                .Where(k => k.Id == id && k.CuaHangId == cuaHangId)
                .Select(k => new
                {
                    k.Id,
                    k.TenKhachHang,
                    k.SoDienThoai,
                    k.Email,
                    k.DiaChi,
                    k.NgaySinh,
                    k.GhiChu,
                    k.TongDiemTichLuy,
                    k.DiemHienTai,
                    k.NgayTao,
                    // Lịch sử mua hàng (Join bảng HoaDon)
                    LichSuMua = _context.HoaDons
                        .Where(h => h.KhachHangId == k.Id && h.TrangThai == "Đã thanh toán")
                        .OrderByDescending(h => h.NgayThanhToan)
                        .Take(20) // Lấy 20 đơn gần nhất
                        .Select(h => new
                        {
                            h.Id,
                            h.NgayThanhToan,
                            h.TongTien,
                            h.PhuongThucThanhToan,
                            TenBan = h.Ban != null ? h.Ban.TenBan : "Mang về"
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            if (khach == null) return NotFound("Không tìm thấy khách hàng!");
            return Ok(khach);
        }

        // ==========================================
        // 3. TÌM NHANH THEO SĐT (DÙNG CHO POS)
        // ==========================================
        [HttpGet("tim-kiem")]
        public async Task<IActionResult> TimKiem([FromQuery] string sdt)
        {
            if (string.IsNullOrEmpty(sdt)) return Ok(new List<object>());

            int cuaHangId = GetCuaHangId();
            var results = await _context.KhachHangs
                .Where(k => k.CuaHangId == cuaHangId && k.SoDienThoai.Contains(sdt))
                .Take(5) // Chỉ trả tối đa 5 kết quả cho dropdown
                .Select(k => new
                {
                    k.Id,
                    k.TenKhachHang,
                    k.SoDienThoai,
                    k.DiemHienTai,
                    k.TongDiemTichLuy
                })
                .ToListAsync();

            return Ok(results);
        }

        // ==========================================
        // 4. THÊM KHÁCH HÀNG MỚI
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Create(KhachHangDto request)
        {
            int cuaHangId = GetCuaHangId();

            // Kiểm tra SĐT trùng trong cùng cửa hàng
            var checkSdt = await _context.KhachHangs
                .AnyAsync(k => k.CuaHangId == cuaHangId && k.SoDienThoai == request.SoDienThoai);
            if (checkSdt) return BadRequest("Số điện thoại này đã được đăng ký cho khách hàng khác!");

            var newKhach = new KhachHang
            {
                CuaHangId = cuaHangId,
                TenKhachHang = request.TenKhachHang,
                SoDienThoai = request.SoDienThoai,
                Email = request.Email,
                DiaChi = request.DiaChi,
                NgaySinh = request.NgaySinh,
                GhiChu = request.GhiChu,
                NgayTao = DateTime.Now,
                TongDiemTichLuy = 0,
                DiemHienTai = 0
            };

            _context.KhachHangs.Add(newKhach);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Thêm khách hàng thành công!",
                id = newKhach.Id,
                tenKhachHang = newKhach.TenKhachHang,
                soDienThoai = newKhach.SoDienThoai,
                diemHienTai = newKhach.DiemHienTai,
                tongDiemTichLuy = newKhach.TongDiemTichLuy
            });
        }

        // ==========================================
        // 5. SỬA KHÁCH HÀNG
        // ==========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, KhachHangDto request)
        {
            int cuaHangId = GetCuaHangId();
            var khach = await _context.KhachHangs
                .FirstOrDefaultAsync(k => k.Id == id && k.CuaHangId == cuaHangId);
            if (khach == null) return NotFound("Không tìm thấy khách hàng!");

            // Kiểm tra SĐT trùng (trừ chính khách này)
            var checkSdt = await _context.KhachHangs
                .AnyAsync(k => k.CuaHangId == cuaHangId && k.SoDienThoai == request.SoDienThoai && k.Id != id);
            if (checkSdt) return BadRequest("Số điện thoại này đã được đăng ký cho khách hàng khác!");

            khach.TenKhachHang = request.TenKhachHang;
            khach.SoDienThoai = request.SoDienThoai;
            khach.Email = request.Email;
            khach.DiaChi = request.DiaChi;
            khach.NgaySinh = request.NgaySinh;
            khach.GhiChu = request.GhiChu;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thông tin khách hàng thành công!" });
        }

        // ==========================================
        // 6. XÓA KHÁCH HÀNG
        // ==========================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            int cuaHangId = GetCuaHangId();
            var khach = await _context.KhachHangs
                .FirstOrDefaultAsync(k => k.Id == id && k.CuaHangId == cuaHangId);
            if (khach == null) return NotFound("Không tìm thấy khách hàng!");

            // Gỡ liên kết khỏi hóa đơn cũ (không xóa hóa đơn)
            var hoaDons = await _context.HoaDons
                .Where(h => h.KhachHangId == id)
                .ToListAsync();
            foreach (var hd in hoaDons)
            {
                hd.KhachHangId = null;
            }

            _context.KhachHangs.Remove(khach);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa khách hàng thành công!" });
        }
    }
}
