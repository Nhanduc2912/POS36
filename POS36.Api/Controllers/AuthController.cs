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
using System.Net.Http;
using System.Threading.Tasks;
using System;

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
            // === VALIDATION ===
            if (string.IsNullOrWhiteSpace(request.TenDangNhap) || request.TenDangNhap.Length < 4)
                return BadRequest("Tên đăng nhập phải có ít nhất 4 ký tự!");
            if (!System.Text.RegularExpressions.Regex.IsMatch(request.TenDangNhap, @"^[a-zA-Z0-9_\.]+$"))
                return BadRequest("Tên đăng nhập chỉ được chứa chữ cái, số, dấu chấm và gạch dưới!");
            if (string.IsNullOrWhiteSpace(request.MatKhau) || request.MatKhau.Length < 6)
                return BadRequest("Mật khẩu phải có ít nhất 6 ký tự!");
            if (!string.IsNullOrEmpty(request.SoDienThoai) &&
                !System.Text.RegularExpressions.Regex.IsMatch(request.SoDienThoai.Trim(), @"^(0[3|5|7|8|9])+([0-9]{8})$"))
                return BadRequest("Số điện thoại không hợp lệ! Nhập SĐT Việt Nam 10 số (VD: 0901234567).");
            if (string.IsNullOrWhiteSpace(request.TenCuaHang) || request.TenCuaHang.Trim().Length < 2)
                return BadRequest("Tên cửa hàng phải có ít nhất 2 ký tự!");
            if (request.TenCuaHang.Length > 100)
                return BadRequest("Tên cửa hàng không được vượt quá 100 ký tự!");

            if (await _context.TaiKhoans.AnyAsync(u => u.TenDangNhap == request.TenDangNhap))
            {
                return BadRequest("Tên đăng nhập đã tồn tại!");
            }

            // Kiểm tra SĐT trùng (nếu có)
            if (!string.IsNullOrEmpty(request.SoDienThoai))
            {
                var sdtExists = await _context.TaiKhoans.AnyAsync(u => u.SoDienThoai == request.SoDienThoai.Trim());
                if (sdtExists) return Conflict("Số điện thoại này đã được đăng ký tài khoản khác!");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Tạo Cửa hàng (SaaS: 7 ngày dùng thử)
                var newCuaHang = new CuaHang
                {
                    TenCuaHang = request.TenCuaHang.Trim(),
                    SoDienThoai = request.SoDienThoai?.Trim(),
                    Email = request.Email?.Trim(),
                    NgayDangKy = DateTime.Now,
                    TrangThai = "DungThu",
                    NgayHetHan = DateTime.Now.AddDays(7)
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

                // Tạo Tài khoản liên kết với Nhân viên (SaaS: lưu Email + SĐT trực tiếp)
                var newTaiKhoan = new TaiKhoan
                {
                    CuaHangId = newCuaHang.Id,
                    NhanVienId = newNhanVien.Id,
                    TenDangNhap = request.TenDangNhap,
                    MatKhauHash = BCrypt.Net.BCrypt.HashPassword(request.MatKhau),
                    VaiTro = "ChuCuaHang",
                    Email = request.Email,
                    SoDienThoai = request.SoDienThoai,
                    NgayTao = DateTime.Now
                };
                _context.TaiKhoans.Add(newTaiKhoan);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                Log.Information("👤 Tài khoản {TenDangNhap} vừa ĐĂNG KÝ thành công vào hệ thống.", request.TenDangNhap);
                return Ok(new { message = "Đăng ký cửa hàng thành công!", cuaHangId = newCuaHang.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Log.Error(ex, "Lỗi khi đăng ký tài khoản cho {TenDangNhap}", request.TenDangNhap);
                return StatusCode(500, $"Lỗi hệ thống khi đăng ký: {ex.Message}");
            }
        }

        // ==========================================
        // 2. LUỒNG ĐĂNG NHẬP
        // ==========================================
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            var user = await _context.TaiKhoans.Include(t => t.NhanVien).FirstOrDefaultAsync(u => u.TenDangNhap == request.TenDangNhap);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.MatKhau, user.MatKhauHash))
            {
                return BadRequest("Sai tên đăng nhập hoặc mật khẩu!");
            }

            // === SaaS: Kiểm tra tài khoản có bị khóa không ===
            if (!user.IsActive)
            {
                return BadRequest(new { message = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên!" });
            }

            // === SaaS: Kiểm tra trạng thái cửa hàng (bỏ qua nếu là SuperAdmin) ===
            string storeTrangThai = "HoatDong";
            if (user.VaiTro != "SuperAdmin")
            {
                var cuaHang = await _context.CuaHangs.FindAsync(user.CuaHangId);
                if (cuaHang != null)
                {
                    storeTrangThai = cuaHang.TrangThai;
                    if (cuaHang.TrangThai == "BiKhoa")
                    {
                        return BadRequest(new { message = "Cửa hàng của bạn đã bị khóa do không gia hạn gói dịch vụ. Vui lòng liên hệ hỗ trợ!" });
                    }
                }
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

            // Cập nhật lần đăng nhập cuối
            user.LanDangNhapCuoi = DateTime.Now;
            await _context.SaveChangesAsync();

            await _context.LogHoatDongAsync(user.ChiNhanhId ?? 0, "Đăng nhập", $"Tài khoản {user.TenDangNhap} đăng nhập thành công vào hệ thống.", user.NhanVien?.TenNhanVien ?? user.TenDangNhap, user.VaiTro, user.CuaHangId);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            Log.Information("👤 Tài khoản {TenDangNhap} vừa ĐĂNG NHẬP thành công.", request.TenDangNhap);

            return Ok(new { 
                token = jwt, 
                role = user.VaiTro, 
                storeTrangThai,
                tenNhanVien = user.NhanVien?.TenNhanVien ?? user.TenDangNhap,
                quyenThuNgan = user.QuyenThuNgan ?? ""
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var claimCuaHangId = User.FindFirst("CuaHangId");
                var claimChiNhanhId = User.FindFirst("ChiNhanhId");
                var claimRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role) ?? User.FindFirst("VaiTro");
                
                int cuaHangId = claimCuaHangId != null ? int.Parse(claimCuaHangId.Value) : 0;
                int chiNhanhId = claimChiNhanhId != null ? int.Parse(claimChiNhanhId.Value) : 0;
                string userRole = claimRole?.Value ?? "Hội viên";
                string userName = User.Identity?.Name ?? User.FindFirst("TenDangNhap")?.Value ?? "NguoiDung";

                if (cuaHangId > 0)
                {
                    await _context.LogHoatDongAsync(chiNhanhId, "Đăng xuất", $"Tài khoản {userName} đăng xuất khỏi hệ thống.", userName, userRole, cuaHangId);
                }
                return Ok(new { message = "Đăng xuất thành công" });
            }
            catch (Exception)
            {
                return Ok(new { message = "Đăng xuất thành công" });
            }
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

            // BUG-04 FIX: Tự động gửi Email từ Backend qua EmailJS REST API
            await SendOtpEmailAsync(request.Email, user.NhanVien!.TenNhanVien, otpCode);

            return Ok(new
            {
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

            // Tự động gửi Email từ Backend qua EmailJS REST API
            await SendOtpEmailAsync(user.NhanVien!.Email ?? "", user.NhanVien.TenNhanVien, otpCode);

            return Ok(new
            {
                tenDangNhap = user.TenDangNhap,
                tenNhanVien = user.NhanVien?.TenNhanVien,
                cooldownSeconds = COOLDOWN_SECONDS  // Frontend dùng để đết ngược countdown
            });
        }

        private async Task<bool> SendOtpEmailAsync(string email, string fullName, string otpCode)
        {
            try
            {
                var serviceId = _configuration["EmailJS:ServiceId"] ?? "service_65xya5u";
                var templateId = _configuration["EmailJS:TemplateId"] ?? "template_a63e1vv";
                var publicKey = _configuration["EmailJS:PublicKey"] ?? "Zjm65dyIcuEbthcT3";
                var privateKey = _configuration["EmailJS:PrivateKey"];

                using var client = new HttpClient();
                var payload = new Dictionary<string, object>
                {
                    { "service_id", serviceId },
                    { "template_id", templateId },
                    { "user_id", publicKey },
                    { "template_params", new
                        {
                            to_email = email,
                            to_name = fullName,
                            otp_code = otpCode
                        }
                    }
                };

                if (!string.IsNullOrEmpty(privateKey))
                {
                    payload["accessToken"] = privateKey;
                }

                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(payload),
                    System.Text.Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync("https://api.emailjs.com/api/v1.0/email/send", content);
                if (response.IsSuccessStatusCode)
                {
                    Log.Information("📧 Gửi mã OTP thành công qua EmailJS tới {Email}", email);
                    return true;
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    Log.Error("❌ Gửi mã OTP thất bại qua EmailJS: {Status} - {Error}", response.StatusCode, errorMsg);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Lỗi hệ thống khi gửi mã OTP qua EmailJS");
                return false;
            }
        }
    }
}
