using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using POS36.Api.DTOs;
using POS36.Api.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Serilog;
using System.Text;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            if (await _context.TaiKhoans.AnyAsync(u => u.TenDangNhap == request.TenDangNhap))
            {
                return BadRequest("Tên đăng nhập đã tồn tại!");
            }

            // 1. Tạo Cửa hàng
            var newCuaHang = new CuaHang
            {
                TenCuaHang = request.TenCuaHang,
                SoDienThoai = request.SoDienThoai,
                NgayDangKy = DateTime.Now
            };

            _context.CuaHangs.Add(newCuaHang);
            await _context.SaveChangesAsync();

            // 2. Tạo Chi nhánh và Khu vực mặc định
            var defaultChiNhanh = new ChiNhanh
            {
                CuaHangId = newCuaHang.Id,
                TenChiNhanh = "Chi nhánh Trung tâm",
                DiaChi = "Chưa cập nhật"
            };
            _context.ChiNhanhs.Add(defaultChiNhanh);
            await _context.SaveChangesAsync();

            var defaultKhuVuc = new KhuVuc
            {
                CuaHangId = newCuaHang.Id,
                ChiNhanhId = defaultChiNhanh.Id,
                TenKhuVuc = "Tầng 1"
            };
            _context.KhuVucs.Add(defaultKhuVuc);
            await _context.SaveChangesAsync();

            // =========================================================
            // 3. FIX LỖI: TẠO HỒ SƠ NHÂN VIÊN CHO CHỦ CỬA HÀNG
            // =========================================================
            var newNhanVien = new NhanVien
            {
                CuaHangId = newCuaHang.Id,
                MaNhanVien = $"ADMIN{DateTime.Now:HHmmss}", // Mã nhân viên ngẫu nhiên chống trùng
                TenNhanVien = request.TenDangNhap, // Tạm lấy tên đăng nhập làm tên hiển thị
                SoDienThoai = request.SoDienThoai,
                Email = ""
            };
            _context.NhanViens.Add(newNhanVien);
            await _context.SaveChangesAsync(); // Sinh ra NhanVien.Id

            // 4. Tạo Tài khoản VÀ LIÊN KẾT VỚI NHÂN VIÊN VỪA TẠO
            var newTaiKhoan = new TaiKhoan
            {
                CuaHangId = newCuaHang.Id,
                NhanVienId = newNhanVien.Id, // <- ĐÂY LÀ CHÌA KHÓA GIẢI QUYẾT LỖI
                TenDangNhap = request.TenDangNhap,
                MatKhauHash = BCrypt.Net.BCrypt.HashPassword(request.MatKhau),
                VaiTro = "ChuCuaHang"
            };

            _context.TaiKhoans.Add(newTaiKhoan);
            await _context.SaveChangesAsync();
            Log.Information("👤 Tài khoản {TenDangNhap} vừa ĐĂNG KÝ thành công vào hệ thống.", request.TenDangNhap);

            return Ok(new { message = "Đăng ký cửa hàng thành công!", cuaHangId = newCuaHang.Id });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            var user = await _context.TaiKhoans.FirstOrDefaultAsync(u => u.TenDangNhap == request.TenDangNhap);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.MatKhau, user.MatKhauHash))
            {
                return BadRequest("Sai tên đăng nhập hoặc mật khẩu!");
            }

            // Tạo danh sách các thông tin nhét vào Token (Claims)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.TenDangNhap),
                new Claim(ClaimTypes.Role, user.VaiTro),
                new Claim("CuaHangId", user.CuaHangId.ToString())
            };

            if (user.ChiNhanhId.HasValue)
            {
                claims.Add(new Claim("ChiNhanhId", user.ChiNhanhId.Value.ToString()));
            }

            // Tạo Token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            Log.Information("👤 Tài khoản {TenDangNhap} vừa ĐĂNG NHẬP thành công vào hệ thống.", request.TenDangNhap);

            return Ok(new { token = jwt, role = user.VaiTro });
        }
    }
}