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

        // 1. LẤY DANH SÁCH NHÂN VIÊN THEO CHI NHÁNH
        // 1. LẤY DANH SÁCH NHÂN VIÊN THEO CHI NHÁNH (Kèm Tài Khoản)
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
                    // Móc sang bảng TaiKhoan để lấy User và Role (Nếu có)
                    TenDangNhap = _context.TaiKhoans.Where(t => t.NhanVienId == nv.Id).Select(t => t.TenDangNhap).FirstOrDefault(),
                    VaiTro = _context.TaiKhoans.Where(t => t.NhanVienId == nv.Id).Select(t => t.VaiTro).FirstOrDefault()
                })
                .ToListAsync();

            return Ok(ds);
        }

        // 2. THÊM NHÂN VIÊN (VÀ CẤP TÀI KHOẢN NẾU CÓ)
        [HttpPost]
        public async Task<IActionResult> Create(NhanVienDto request)
        {
            int cuaHangId = GetCuaHangId();

            // Nếu muốn tạo tài khoản, phải kiểm tra xem Tên đăng nhập đã bị ai xài chưa
            if (request.TaoTaiKhoan)
            {
                if (string.IsNullOrEmpty(request.TenDangNhap) || string.IsNullOrEmpty(request.MatKhau))
                    return BadRequest("Vui lòng nhập đầy đủ Tên đăng nhập và Mật khẩu!");

                var checkUsername = await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == request.TenDangNhap);
                if (checkUsername) return BadRequest("Tên đăng nhập đã tồn tại trên hệ thống!");
            }

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
                    SoDienThoai = request.SoDienThoai
                };
                _context.NhanViens.Add(newNv);
                await _context.SaveChangesAsync(); // Lưu để lấy newNv.Id

                // Bước 2: Tạo Tài khoản (Nếu được yêu cầu)
                if (request.TaoTaiKhoan)
                {
                    var newTaiKhoan = new TaiKhoan
                    {
                        CuaHangId = cuaHangId,
                        ChiNhanhId = request.ChiNhanhId, // Gắn vào nhánh để mốt dễ quản lý
                        NhanVienId = newNv.Id,           // MÓC NỐI VÀO BẢNG NHÂN VIÊN
                        TenDangNhap = request.TenDangNhap!,
                        VaiTro = request.VaiTro ?? "ThuNgan", // Mặc định là thu ngân
                        // Lưu ý: Thực tế chỗ này phải dùng BCrypt để mã hóa (Hash), anh tạm dùng mã hóa BCrypt mặc định
                        MatKhauHash = BCrypt.Net.BCrypt.HashPassword(request.MatKhau)
                    };
                    _context.TaiKhoans.Add(newTaiKhoan);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return Ok(new { message = "Thêm nhân viên thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // 3. SỬA NHÂN VIÊN
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, NhanVienDto request)
        {
            int cuaHangId = GetCuaHangId();
            var nv = await _context.NhanViens.FirstOrDefaultAsync(n => n.Id == id && n.CuaHangId == cuaHangId);
            if (nv == null) return NotFound("Không tìm thấy nhân viên!");

            nv.MaNhanVien = request.MaNhanVien;
            nv.TenNhanVien = request.TenNhanVien;
            nv.SoDienThoai = request.SoDienThoai;
            nv.ChiNhanhId = request.ChiNhanhId;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công!" });
        }

        // 4. XÓA NHÂN VIÊN
        // 4. XÓA NHÂN VIÊN VÀ THU HỒI TÀI KHOẢN
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            int cuaHangId = GetCuaHangId();
            var nv = await _context.NhanViens.FirstOrDefaultAsync(n => n.Id == id && n.CuaHangId == cuaHangId);
            if (nv == null) return NotFound("Không tìm thấy nhân viên!");

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