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
    public class NhanVienController : ControllerBase
    {
        private readonly AppDbContext _context;
        public NhanVienController(AppDbContext context) { _context = context; }

        private int GetCuaHangId() => int.Parse(User.FindFirst("CuaHangId")!.Value);

        // 1. LẤY DANH SÁCH NHÂN VIÊN THEO CHI NHÁNH (Kèm Tài Khoản + Email)
        [HttpGet("danh-sach")]
        public async Task<IActionResult> GetDanhSach([FromQuery] int chiNhanhId)
        {
            int cuaHangId = GetCuaHangId();

            var ds = await _context.NhanViens
                .Where(nv => nv.CuaHangId == cuaHangId && nv.ChiNhanhId == chiNhanhId)
                .Select(nv => new
                {
                    nv.Id,
                    nv.MaNhanVien,
                    nv.TenNhanVien,
                    nv.SoDienThoai,
                    nv.Email, // FIX: Thêm Email vào danh sách trả về
                    TenDangNhap = _context.TaiKhoans.Where(t => t.NhanVienId == nv.Id).Select(t => t.TenDangNhap).FirstOrDefault(),
                    VaiTro = _context.TaiKhoans.Where(t => t.NhanVienId == nv.Id).Select(t => t.VaiTro).FirstOrDefault()
                })
                .ToListAsync();

            return Ok(ds);
        }

        // 2. THÊM NHÂN VIÊN (BẮT BUỘC CẤP TÀI KHOẢN)
        // BUG #12 FIX: Chỉ ChuCuaHang mới được thêm nhân viên
        [Authorize(Roles = "ChuCuaHang")]
        [HttpPost]
        public async Task<IActionResult> Create(NhanVienDto request)
        {
            int cuaHangId = GetCuaHangId();

            // VALIDATION 1: Bắt buộc phải cấp tài khoản và vai trò
            if (string.IsNullOrWhiteSpace(request.VaiTro))
                return BadRequest(new { message = "Vui lòng chọn Vai trò cho nhân viên!" });

            if (string.IsNullOrWhiteSpace(request.TenDangNhap) || string.IsNullOrWhiteSpace(request.MatKhau))
                return BadRequest(new { message = "Vui lòng nhập đầy đủ Tên đăng nhập và Mật khẩu!" });

            // VALIDATION 2: Kiểm tra Mã Nhân viên trùng lặp
            bool maNvTrung = await _context.NhanViens.AnyAsync(
                nv => nv.CuaHangId == cuaHangId && nv.MaNhanVien == request.MaNhanVien);
            if (maNvTrung)
                return BadRequest(new { message = $"Mã nhân viên '{request.MaNhanVien}' đã tồn tại trên hệ thống!" });

            // VALIDATION 3: Kiểm tra Số Điện Thoại trùng lặp
            bool sdtTrung = await _context.NhanViens.AnyAsync(
                nv => nv.CuaHangId == cuaHangId && nv.SoDienThoai == request.SoDienThoai);
            if (sdtTrung)
                return BadRequest(new { message = $"Số điện thoại '{request.SoDienThoai}' đã được đăng ký cho nhân viên khác!" });

            // VALIDATION 4: Tên đăng nhập đã tồn tại chưa
            bool usernameTrung = await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == request.TenDangNhap);
            if (usernameTrung)
                return BadRequest(new { message = $"Tên đăng nhập '{request.TenDangNhap}' đã tồn tại trên hệ thống!" });

            // Bắt đầu Transaction (Lưu an toàn vào 2 bảng)
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Bước 1: Tạo Hồ sơ Nhân viên
                var newNv = new NhanVien
                {
                    CuaHangId = cuaHangId,
                    ChiNhanhId = request.ChiNhanhId,
                    MaNhanVien = request.MaNhanVien,
                    TenNhanVien = request.TenNhanVien,
                    SoDienThoai = request.SoDienThoai,
                    Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email // FIX: Lưu Email
                };
                _context.NhanViens.Add(newNv);
                await _context.SaveChangesAsync(); // Lưu để lấy newNv.Id

                // Bước 2: Tạo Tài khoản (Bắt buộc)
                var newTaiKhoan = new TaiKhoan
                {
                    CuaHangId = cuaHangId,
                    ChiNhanhId = request.ChiNhanhId,
                    NhanVienId = newNv.Id,
                    TenDangNhap = request.TenDangNhap!,
                    VaiTro = request.VaiTro,
                    MatKhauHash = BCrypt.Net.BCrypt.HashPassword(request.MatKhau)
                };
                _context.TaiKhoans.Add(newTaiKhoan);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return Ok(new { message = "Thêm nhân viên thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // 3. SỬA NHÂN VIÊN (Chỉ sửa Tên, SĐT, Email — KHÔNG cho sửa Mã NV)
        // BUG #12 FIX: Chỉ ChuCuaHang mới được sửa nhân viên
        [Authorize(Roles = "ChuCuaHang")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, NhanVienDto request)
        {
            int cuaHangId = GetCuaHangId();
            var nv = await _context.NhanViens.FirstOrDefaultAsync(n => n.Id == id && n.CuaHangId == cuaHangId);
            if (nv == null) return NotFound(new { message = "Không tìm thấy nhân viên!" });

            // VALIDATION: Kiểm tra Số Điện Thoại trùng với nhân viên KHÁC
            bool sdtTrung = await _context.NhanViens.AnyAsync(
                n => n.CuaHangId == cuaHangId && n.SoDienThoai == request.SoDienThoai && n.Id != id);
            if (sdtTrung)
                return BadRequest(new { message = $"Số điện thoại '{request.SoDienThoai}' đã được đăng ký cho nhân viên khác!" });

            // FIX: Chỉ cập nhật Tên, SĐT, Email — KHÔNG cập nhật MaNhanVien
            nv.TenNhanVien = request.TenNhanVien;
            nv.SoDienThoai = request.SoDienThoai;
            nv.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email;
            // nv.MaNhanVien = ... ← KHÔNG cho sửa mã NV

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công!" });
        }

        // 4. XÓA NHÂN VIÊN VÀ THU HỒI TÀI KHOẢN
        // BUG #12 FIX: Chỉ ChuCuaHang mới được xóa nhân viên
        [Authorize(Roles = "ChuCuaHang")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            int cuaHangId = GetCuaHangId();
            var nv = await _context.NhanViens.FirstOrDefaultAsync(n => n.Id == id && n.CuaHangId == cuaHangId);
            if (nv == null) return NotFound(new { message = "Không tìm thấy nhân viên!" });

            // 1. TÌM VÀ XÓA TÀI KHOẢN LIÊN KẾT TRƯỚC (Chống lỗi Khóa ngoại)
            var taiKhoan = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.NhanVienId == id);
            if (taiKhoan != null)
            {
                _context.TaiKhoans.Remove(taiKhoan);
            }

            // 2. SAU ĐÓ MỚI XÓA HỒ SƠ NHÂN VIÊN
            _context.NhanViens.Remove(nv);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa nhân viên và thu hồi tài khoản thành công!" });
        }
    }
}