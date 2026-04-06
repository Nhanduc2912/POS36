using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using POS36.Api.DTOs; // Import thư mục DTO của sếp vào đây
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

        // BỘ NHỚ RAM ĐỂ LƯU TẠM MÃ OTP (Sẽ mất khi tắt server)
        private static readonly Dictionary<string, string> _otpCache = new Dictionary<string, string>();

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // ==========================================
        // 1. LUỒNG ĐĂNG KÝ
        // ==========================================
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            if (await _context.TaiKhoans.AnyAsync(u => u.TenDangNhap == request.TenDangNhap))
            {
                return BadRequest("Tên đăng nhập đã tồn tại!");
            }

            // Tạo Cửa hàng
            var newCuaHang = new CuaHang
            {
                TenCuaHang = request.TenCuaHang,
                SoDienThoai = request.SoDienThoai,
                NgayDangKy = DateTime.Now
            };
            _context.CuaHangs.Add(newCuaHang);
            await _context.SaveChangesAsync();

            // Tạo Chi nhánh mặc định
            var defaultChiNhanh = new ChiNhanh
            {
                CuaHangId = newCuaHang.Id,
                TenChiNhanh = "Chi nhánh Trung tâm",
                DiaChi = "Chưa cập nhật"
            };
            _context.ChiNhanhs.Add(defaultChiNhanh);
            await _context.SaveChangesAsync();

            // Tạo Khu vực mặc định
            var defaultKhuVuc = new KhuVuc
            {
                CuaHangId = newCuaHang.Id,
                ChiNhanhId = defaultChiNhanh.Id,
                TenKhuVuc = "Tầng 1"
            };
            _context.KhuVucs.Add(defaultKhuVuc);
            await _context.SaveChangesAsync();

            // TẠO HỒ SƠ NHÂN VIÊN (Dành cho Chủ cửa hàng)
            var newNhanVien = new NhanVien
            {
                CuaHangId = newCuaHang.Id,
                MaNhanVien = $"ADMIN{DateTime.Now:HHmmss}",
                TenNhanVien = request.TenDangNhap,
                SoDienThoai = request.SoDienThoai,
                Email = request.Email // ĐÃ LƯU EMAIL VÀO ĐÂY
            };
            _context.NhanViens.Add(newNhanVien);
            await _context.SaveChangesAsync();

            // Tạo Tài khoản liên kết với Nhân viên
            var newTaiKhoan = new TaiKhoan
            {
                CuaHangId = newCuaHang.Id,
                NhanVienId = newNhanVien.Id,
                TenDangNhap = request.TenDangNhap,
                MatKhauHash = BCrypt.Net.BCrypt.HashPassword(request.MatKhau),
                VaiTro = "ChuCuaHang"
            };
            _context.TaiKhoans.Add(newTaiKhoan);
            await _context.SaveChangesAsync();

            Log.Information("👤 Tài khoản {TenDangNhap} vừa ĐĂNG KÝ thành công vào hệ thống.", request.TenDangNhap);
            return Ok(new { message = "Đăng ký cửa hàng thành công!", cuaHangId = newCuaHang.Id });
        }

        // ==========================================
        // 2. LUỒNG ĐĂNG NHẬP
        // ==========================================
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            var user = await _context.TaiKhoans.FirstOrDefaultAsync(u => u.TenDangNhap == request.TenDangNhap);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.MatKhau, user.MatKhauHash))
            {
                return BadRequest("Sai tên đăng nhập hoặc mật khẩu!");
            }

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
            Log.Information("👤 Tài khoản {TenDangNhap} vừa ĐĂNG NHẬP thành công.", request.TenDangNhap);

            return Ok(new { token = jwt, role = user.VaiTro });
        }

        // ==========================================
        // 3. LUỒNG QUÊN MẬT KHẨU (GỬI OTP QUA EMAIL)
        // ==========================================
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            // TÌM TÀI KHOẢN CHỦ CỬA HÀNG QUA EMAIL TRONG BẢNG NHANVIEN
            var user = await _context.TaiKhoans
                .Include(t => t.NhanVien)
                .FirstOrDefaultAsync(t => t.NhanVien != null && t.NhanVien.Email == request.Email && t.VaiTro == "ChuCuaHang");

            if (user == null)
            {
                return BadRequest("Không tìm thấy Chủ cửa hàng nào sử dụng Email này!");
            }

            string otpCode = new Random().Next(100000, 999999).ToString();
            _otpCache[user.TenDangNhap] = otpCode; // Lưu OTP vào cache bằng TenDangNhap

            // TRẢ VỀ CHO VUE ĐỂ VUE BẮN EMAILJS
            return Ok(new
            {
                otp = otpCode,
                tenDangNhap = user.TenDangNhap,
                tenNhanVien = user.NhanVien!.TenNhanVien
            });
        }

        // ==========================================
        // 4. LUỒNG XÁC NHẬN OTP & ĐỔI MẬT KHẨU
        // ==========================================
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!_otpCache.TryGetValue(request.TenDangNhap, out string? savedOtp) || savedOtp != request.OtpCode)
            {
                return BadRequest("Mã xác nhận OTP không chính xác hoặc đã hết hạn!");
            }

            var user = await _context.TaiKhoans.FirstOrDefaultAsync(u => u.TenDangNhap == request.TenDangNhap);
            if (user != null)
            {
                user.MatKhauHash = BCrypt.Net.BCrypt.HashPassword(request.MatKhauMoi);
                await _context.SaveChangesAsync();
            }

            _otpCache.Remove(request.TenDangNhap);

            Log.Information("👤 Tài khoản {TenDangNhap} vừa ĐẶT LẠI MẬT KHẨU thành công.", request.TenDangNhap);
            return Ok(new { message = "Đổi mật khẩu thành công! Bạn có thể đăng nhập ngay." });
        }
    }
}