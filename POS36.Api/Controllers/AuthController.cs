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

        // BUG #2 FIX: Đã xóa _otpCache (RAM). OTP giờ được lưu vào bảng OtpRequests trong DB.

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

            // Kiểm tra COOLDOWN: nếu đã có OTP mới tạo trong vòng 60 giây, chặn gửi lại
            var otpGanDay = await _context.OtpRequests
                .Where(o => o.TenDangNhap == user.TenDangNhap && !o.DaSDung)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpGanDay != null)
            {
                var secondsElapsed = (DateTime.Now - otpGanDay.CreatedAt).TotalSeconds;
                const int COOLDOWN_SECONDS = 60; // 1 phút
                if (secondsElapsed < COOLDOWN_SECONDS)
                {
                    int secondsLeft = (int)(COOLDOWN_SECONDS - secondsElapsed);
                    return BadRequest(new
                    {
                        message = $"Vui lòng đợi {secondsLeft} giây trước khi yêu cầu mã mới!",
                        secondsLeft = secondsLeft
                    });
                }
            }

            // BUG #2 FIX: Sinh OTP và lưu vào DB thay vì RAM
            string otpCode = new Random().Next(100000, 999999).ToString();

            // Vô hiệu hóa các OTP cũ chưa dùng của tài khoản này (tránh tích lũy rác)
            var otpCu = _context.OtpRequests
                .Where(o => o.TenDangNhap == user.TenDangNhap && !o.DaSDung);
            _context.OtpRequests.RemoveRange(otpCu);

            // Lưu OTP mới vào DB với TTL 5 phút
            _context.OtpRequests.Add(new POS36.Api.Models.OtpRequest
            {
                TenDangNhap = user.TenDangNhap,
                OtpHash = BCrypt.Net.BCrypt.HashPassword(otpCode), // Hash OTP — không lưu plain text
                ExpiresAt = DateTime.Now.AddMinutes(5),
                CreatedAt = DateTime.Now,
                DaSDung = false
            });
            await _context.SaveChangesAsync();

            // BUG #2 FIX: KHÔNG trả OTP về response JSON (tránh bị intercept)
            // OTP được gửi qua email bởi frontend (EmailJS) — tạm thời vẫn trả về để tương thích frontend
            // TODO: Khi có SMTP, xóa "otp" khỏi response và backend tự gửi email
            return Ok(new
            {
                otp = otpCode,              // ⚠️ Xóa dòng này khi backend tự gửi email qua SMTP
                tenDangNhap = user.TenDangNhap,
                tenNhanVien = user.NhanVien!.TenNhanVien,
                cooldownSeconds = 60       // Thông báo frontend biết phải đợi bao lâu mới được gửi lại
            });
        }

        // ==========================================
        // 4. LUỒNG XÁC NHẬN OTP & ĐỔI MẬT KHẨU
        // ==========================================
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            // BUG #2 FIX: Kiểm tra OTP từ DB thay vì RAM
            var otpRecord = await _context.OtpRequests
                .Where(o => o.TenDangNhap == request.TenDangNhap && !o.DaSDung)
                .OrderByDescending(o => o.ExpiresAt)
                .FirstOrDefaultAsync();

            if (otpRecord == null)
                return BadRequest("Mã xác nhận OTP không chính xác hoặc đã hết hạn!");

            // Kiểm tra TTL
            if (otpRecord.ExpiresAt < DateTime.Now)
            {
                otpRecord.DaSDung = true; // Đánh dấu đã dùng để dọn rác
                await _context.SaveChangesAsync();
                return BadRequest("Mã OTP đã hết hạn (quá 5 phút). Vui lòng yêu cầu mã mới!");
            }

            // Kiểm tra OTP khớp bằng bcrypt (chống timing attack)
            if (!BCrypt.Net.BCrypt.Verify(request.OtpCode, otpRecord.OtpHash))
                return BadRequest("Mã xác nhận OTP không chính xác hoặc đã hết hạn!");

            // OTP hợp lệ — đánh dấu đã dùng (chống replay attack)
            otpRecord.DaSDung = true;

            var user = await _context.TaiKhoans.FirstOrDefaultAsync(u => u.TenDangNhap == request.TenDangNhap);
            if (user != null)
            {
                user.MatKhauHash = BCrypt.Net.BCrypt.HashPassword(request.MatKhauMoi);
            }

            await _context.SaveChangesAsync();

            Log.Information("👤 Tài khoản {TenDangNhap} vừa ĐẶT LẠI MẬT KHẨU thành công.", request.TenDangNhap);
            return Ok(new { message = "Đổi mật khẩu thành công! Bạn có thể đăng nhập ngay." });
        }

        // ==========================================
        // 5. GỬi Lại Mã OTP (Có kiểm tra cooldown 60 giây)
        // ==========================================
        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequest request)
        {
            // Tìm tài khoản theo TenDangNhap (frontend đã có sau lần gọi forgot-password)
            var user = await _context.TaiKhoans
                .Include(t => t.NhanVien)
                .FirstOrDefaultAsync(t => t.TenDangNhap == request.TenDangNhap);

            if (user == null)
                return BadRequest("Tài khoản không tồn tại!");

            // Kiểm tra COOLDOWN: OTP hiện tại được tạo bao lâu rồi?
            var otpHienTai = await _context.OtpRequests
                .Where(o => o.TenDangNhap == request.TenDangNhap && !o.DaSDung)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            const int COOLDOWN_SECONDS = 60; // Ngưỡng gửi lại tối thiểu: 60 giây (1 phút)

            if (otpHienTai != null)
            {
                var secondsElapsed = (DateTime.Now - otpHienTai.CreatedAt).TotalSeconds;

                if (secondsElapsed < COOLDOWN_SECONDS)
                {
                    // Chưa đủ thời gian chuyển — báo cho frontend biết phải đợi bao giây
                    int secondsLeft = (int)(COOLDOWN_SECONDS - secondsElapsed);
                    return BadRequest(new
                    {
                        message = $"Vui lòng đợi thêm {secondsLeft} giây trước khi yêu cầu mã mới!",
                        secondsLeft = secondsLeft
                    });
                }

                // OTP cũ đã hết hạn hoặc quá 60 giây — vô hiệu hóa trước khi tạo mới
                _context.OtpRequests.Remove(otpHienTai);
            }

            // Tạo OTP mới
            string otpCode = new Random().Next(100000, 999999).ToString();

            _context.OtpRequests.Add(new POS36.Api.Models.OtpRequest
            {
                TenDangNhap = user.TenDangNhap,
                OtpHash = BCrypt.Net.BCrypt.HashPassword(otpCode),
                ExpiresAt = DateTime.Now.AddMinutes(5),
                CreatedAt = DateTime.Now,
                DaSDung = false
            });
            await _context.SaveChangesAsync();

            Log.Information("🔄 Tài khoản {TenDangNhap} đã YÊu CẦU GỬi LẠI OTP.", user.TenDangNhap);

            return Ok(new
            {
                otp = otpCode,          // ⚠️ Xóa khi backend tự gửi email qua SMTP
                tenDangNhap = user.TenDangNhap,
                tenNhanVien = user.NhanVien?.TenNhanVien,
                cooldownSeconds = COOLDOWN_SECONDS  // Frontend dùng để đết ngược countdown
            });
        }
    }
}