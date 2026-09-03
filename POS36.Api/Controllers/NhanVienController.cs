using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using POS36.Api.Data;
using POS36.Api.DTOs;
using POS36.Api.Hubs;
using POS36.Api.Models;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NhanVienController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<KitchenHub> _hubContext;

        public NhanVienController(AppDbContext context, IHubContext<KitchenHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        private int GetCuaHangId() => int.Parse(User.FindFirst("CuaHangId")!.Value);

        // 1. LẤY DANH SÁCH NHÂN VIÊN THEO CHI NHÁNH (Kèm Tài Khoản + Email)
        [HttpGet("danh-sach")]
        public async Task<IActionResult> GetDanhSach([FromQuery] int chiNhanhId)
        {
            int cuaHangId = GetCuaHangId();

            var ds = await _context.NhanViens
                .Where(nv => nv.CuaHangId == cuaHangId && nv.ChiNhanhId == chiNhanhId && !nv.IsDeleted)
                .Select(nv => new
                {
                    nv.Id,
                    nv.MaNhanVien,
                    nv.TenNhanVien,
                    nv.SoDienThoai,
                    nv.Email,
                    nv.Cccd,
                    nv.NgayCapCccd,
                    nv.NoiCapCccd,
                    nv.GioiTinh,
                    nv.NgaySinh,
                    nv.DiaChiThuongTru,
                    nv.DiaChiTamTru,
                    nv.NgayVaoLam,
                    nv.NguoiLienHeKhanCap,
                    nv.SdtKhanCap,
                    nv.MoiQuanHeKhanCap,
                    nv.DongYXuLyDuLieu,
                    nv.NgayDongY,
                    TenDangNhap = _context.TaiKhoans.Where(t => t.NhanVienId == nv.Id).Select(t => t.TenDangNhap).FirstOrDefault(),
                    VaiTro = _context.TaiKhoans.Where(t => t.NhanVienId == nv.Id).Select(t => t.VaiTro).FirstOrDefault(),
                    IsActive = _context.TaiKhoans.Where(t => t.NhanVienId == nv.Id).Select(t => t.IsActive).FirstOrDefault(),
                    QuyenThuNgan = _context.TaiKhoans.Where(t => t.NhanVienId == nv.Id).Select(t => t.QuyenThuNgan).FirstOrDefault()
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

            // VALIDATION 0: Xác thực chi nhánh có thuộc cửa hàng hiện tại hay không để chống IDOR
            var checkChiNhanh = await _context.ChiNhanhs.AnyAsync(cn => cn.Id == request.ChiNhanhId && cn.CuaHangId == cuaHangId);
            if (!checkChiNhanh)
                return BadRequest(new { message = "Chi nhánh không hợp lệ hoặc không thuộc cửa hàng của bạn!" });

            // VALIDATION 1: Bắt buộc phải cấp tài khoản và vai trò
            if (string.IsNullOrWhiteSpace(request.VaiTro))
                return BadRequest(new { message = "Vui lòng chọn Vai trò cho nhân viên!" });

            if (string.IsNullOrWhiteSpace(request.TenDangNhap) || string.IsNullOrWhiteSpace(request.MatKhau))
                return BadRequest(new { message = "Vui lòng nhập đầy đủ Tên đăng nhập và Mật khẩu!" });

            // VALIDATION 1.5: Thông tin hồ sơ pháp lý bắt buộc
            if (string.IsNullOrWhiteSpace(request.Cccd) || request.NgaySinh == null || string.IsNullOrWhiteSpace(request.GioiTinh) || string.IsNullOrWhiteSpace(request.DiaChiThuongTru) || string.IsNullOrWhiteSpace(request.DiaChiTamTru))
                return BadRequest(new { message = "Vui lòng nhập đầy đủ các thông tin hồ sơ pháp lý bắt buộc (CCCD, Ngày sinh, Giới tính, Thường trú, Tạm trú)!" });

            // VALIDATION 1.6: Phải có sự đồng ý xử lý dữ liệu cá nhân (NĐ 13/2023/NĐ-CP)
            if (!request.DongYXuLyDuLieu)
                return BadRequest(new { message = "Nhân viên phải đồng ý cho phép xử lý dữ liệu cá nhân theo Nghị định 13/2023/NĐ-CP trước khi tạo hồ sơ!" });

            // VALIDATION 2: Tự động sinh Mã Nhân viên
            int countNv = await _context.NhanViens.CountAsync(nv => nv.CuaHangId == cuaHangId);
            string generatedMaNv = $"NV{(countNv + 1):D4}";
            
            // Đảm bảo không trùng (phòng trường hợp đã xóa bớt)
            while (await _context.NhanViens.AnyAsync(nv => nv.CuaHangId == cuaHangId && nv.MaNhanVien == generatedMaNv))
            {
                countNv++;
                generatedMaNv = $"NV{(countNv + 1):D4}";
            }
            request.MaNhanVien = generatedMaNv;

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
                    Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email, // FIX: Lưu Email
                    Cccd = request.Cccd,
                    NgayCapCccd = request.NgayCapCccd,
                    NoiCapCccd = request.NoiCapCccd,
                    GioiTinh = request.GioiTinh,
                    NgaySinh = request.NgaySinh,
                    DiaChiThuongTru = request.DiaChiThuongTru,
                    DiaChiTamTru = request.DiaChiTamTru,
                    NgayVaoLam = request.NgayVaoLam,
                    NguoiLienHeKhanCap = request.NguoiLienHeKhanCap,
                    SdtKhanCap = request.SdtKhanCap,
                    MoiQuanHeKhanCap = request.MoiQuanHeKhanCap,
                    DongYXuLyDuLieu = request.DongYXuLyDuLieu,
                    NgayDongY = request.DongYXuLyDuLieu ? DateTime.Now : null
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

            // VALIDATION 1.5: Thông tin hồ sơ pháp lý bắt buộc
            if (string.IsNullOrWhiteSpace(request.Cccd) || request.NgaySinh == null || string.IsNullOrWhiteSpace(request.GioiTinh) || string.IsNullOrWhiteSpace(request.DiaChiThuongTru) || string.IsNullOrWhiteSpace(request.DiaChiTamTru))
                return BadRequest(new { message = "Vui lòng nhập đầy đủ các thông tin hồ sơ pháp lý bắt buộc (CCCD, Ngày sinh, Giới tính, Thường trú, Tạm trú)!" });

            // FIX: Chỉ cập nhật Tên, SĐT, Email — KHÔNG cập nhật MaNhanVien
            nv.TenNhanVien = request.TenNhanVien;
            nv.SoDienThoai = request.SoDienThoai;
            nv.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email;
            nv.Cccd = request.Cccd;
            nv.NgayCapCccd = request.NgayCapCccd;
            nv.NoiCapCccd = request.NoiCapCccd;
            nv.GioiTinh = request.GioiTinh;
            nv.NgaySinh = request.NgaySinh;
            nv.DiaChiThuongTru = request.DiaChiThuongTru;
            nv.DiaChiTamTru = request.DiaChiTamTru;
            nv.NgayVaoLam = request.NgayVaoLam;
            nv.NguoiLienHeKhanCap = request.NguoiLienHeKhanCap;
            nv.SdtKhanCap = request.SdtKhanCap;
            nv.MoiQuanHeKhanCap = request.MoiQuanHeKhanCap;
            // nv.MaNhanVien = ... ← KHÔNG cho sửa mã NV

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công!" });
        }

        // 4. XÓA MỀM NHÂN VIÊN (IsDeleted = true, vô hiệu hóa tài khoản)
        [Authorize(Roles = "ChuCuaHang")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            int cuaHangId = GetCuaHangId();
            var nv = await _context.NhanViens.FirstOrDefaultAsync(n => n.Id == id && n.CuaHangId == cuaHangId && !n.IsDeleted);
            if (nv == null) return NotFound(new { message = "Không tìm thấy nhân viên!" });

            // Soft delete: đánh dấu đã xóa thay vì xóa khỏi DB
            nv.IsDeleted = true;
            nv.NgayXoa = DateTime.Now;
            nv.NguoiXoa = User.FindFirst("TenDangNhap")?.Value ?? User.Identity?.Name;

            // Vô hiệu hóa tài khoản liên kết (không xóa, để giữ lịch sử)
            var taiKhoan = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.NhanVienId == id);
            if (taiKhoan != null)
            {
                taiKhoan.IsActive = false;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã xóa nhân viên và vô hiệu hóa tài khoản thành công!" });
        }

        // 5. KHÔI PHỤC NHÂN VIÊN ĐÃ XÓA
        [Authorize(Roles = "ChuCuaHang")]
        [HttpPut("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            int cuaHangId = GetCuaHangId();
            var nv = await _context.NhanViens.FirstOrDefaultAsync(n => n.Id == id && n.CuaHangId == cuaHangId && n.IsDeleted);
            if (nv == null) return NotFound(new { message = "Không tìm thấy nhân viên đã xóa!" });

            nv.IsDeleted = false;
            nv.NgayXoa = null;
            nv.NguoiXoa = null;

            // Mở lại tài khoản
            var taiKhoan = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.NhanVienId == id);
            if (taiKhoan != null) taiKhoan.IsActive = true;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Khôi phục nhân viên thành công!" });
        }

        // 6. TOGGLE TRẠNG THÁI HOẠT ĐỘNG (Kích hoạt/Vô hiệu hóa tài khoản)
        [Authorize(Roles = "SuperAdmin,ChuCuaHang")]
        [HttpPut("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            int cuaHangId = GetCuaHangId();
            var taiKhoan = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.NhanVienId == id && t.CuaHangId == cuaHangId);
            if (taiKhoan == null) return NotFound(new { message = "Không tìm thấy tài khoản nhân viên!" });

            taiKhoan.IsActive = !taiKhoan.IsActive;
            taiKhoan.SecurityStamp = Guid.NewGuid().ToString(); // Thu hồi token hiện tại
            await _context.SaveChangesAsync();

            // Bắn SignalR thông báo ép đăng xuất nếu bị vô hiệu hóa
            await _hubContext.Clients.Group($"store_{cuaHangId}").SendAsync("QuyenThuNganDaThayDoi", new {
                nhanVienId = id,
                taiKhoanId = taiKhoan.Id,
                isActive = taiKhoan.IsActive,
                forceLogout = true
            });

            return Ok(new { 
                message = taiKhoan.IsActive ? "Đã kích hoạt tài khoản thành công!" : "Đã khóa tài khoản thành công!",
                isActive = taiKhoan.IsActive 
            });
        }

        // 7. LẤY QUYỀN ADMIN CỦA THU NGÂN
        [Authorize(Roles = "ChuCuaHang")]
        [HttpGet("{id}/quyen-admin")]
        public async Task<IActionResult> GetQuyenAdmin(int id)
        {
            int cuaHangId = GetCuaHangId();
            var taiKhoan = await _context.TaiKhoans.FirstOrDefaultAsync(
                t => t.NhanVienId == id && t.CuaHangId == cuaHangId && t.VaiTro == "ThuNgan");
            if (taiKhoan == null) return NotFound(new { message = "Không tìm thấy tài khoản Thu ngân!" });

            return Ok(new { quyenThuNgan = taiKhoan.QuyenThuNgan ?? "" });
        }

        // 8. CẬP NHẬT QUYỀN ADMIN CỦA THU NGÂN (Chủ cửa hàng phân quyền)
        [Authorize(Roles = "ChuCuaHang")]
        [HttpPut("{id}/quyen-admin")]
        public async Task<IActionResult> UpdateQuyenAdmin(int id, [FromBody] UpdateQuyenDto request)
        {
            int cuaHangId = GetCuaHangId();
            var taiKhoan = await _context.TaiKhoans.FirstOrDefaultAsync(
                t => t.NhanVienId == id && t.CuaHangId == cuaHangId && t.VaiTro == "ThuNgan");
            if (taiKhoan == null) return NotFound(new { message = "Chỉ có thể phân quyền cho tài khoản Thu ngân!" });

            taiKhoan.QuyenThuNgan = string.IsNullOrWhiteSpace(request.QuyenThuNgan) ? null : request.QuyenThuNgan.Trim();
            taiKhoan.SecurityStamp = Guid.NewGuid().ToString(); // Thu hồi token cũ
            await _context.SaveChangesAsync();

            // Phát SignalR thông báo ép đăng xuất ngay lập tức tới máy Thu ngân
            await _hubContext.Clients.Group($"store_{cuaHangId}").SendAsync("QuyenThuNganDaThayDoi", new {
                nhanVienId = id,
                taiKhoanId = taiKhoan.Id,
                quyenThuNgan = taiKhoan.QuyenThuNgan,
                forceLogout = true
            });

            return Ok(new { message = "Đã cập nhật quyền Admin thành công! Hệ thống đã yêu cầu tài khoản Thu ngân đăng xuất để áp dụng quyền mới.", quyenThuNgan = taiKhoan.QuyenThuNgan });
        }
    }
}