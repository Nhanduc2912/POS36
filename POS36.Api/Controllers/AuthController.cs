using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using POS36.Api.DTOs;
using POS36.Api.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        // Bơm IConfiguration vào để đọc appsettings.json
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

            var newCuaHang = new CuaHang
            {
                TenCuaHang = request.TenCuaHang,
                SoDienThoai = request.SoDienThoai,
                NgayDangKy = DateTime.Now
            };

            _context.CuaHangs.Add(newCuaHang);
            await _context.SaveChangesAsync();

            // Tự động tạo một Chi nhánh mặc định và Khu vực mặc định cho cửa hàng mới
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

            var newTaiKhoan = new TaiKhoan
            {
                CuaHangId = newCuaHang.Id,
                TenDangNhap = request.TenDangNhap,
                MatKhauHash = BCrypt.Net.BCrypt.HashPassword(request.MatKhau),
                VaiTro = "ChuCuaHang"
            };

            _context.TaiKhoans.Add(newTaiKhoan);
            await _context.SaveChangesAsync();

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
                new Claim("CuaHangId", user.CuaHangId.ToString()) // Nhét CuaHangId vào để sau này biết của quán nào
            };

            // Nếu user thuộc 1 chi nhánh cụ thể thì nhét thêm ChiNhanhId vào
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
                expires: DateTime.Now.AddDays(1), // Token sống 1 ngày
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            // Trả Token về cho người dùng
            return Ok(new { token = jwt, role = user.VaiTro });
        }
    }
}