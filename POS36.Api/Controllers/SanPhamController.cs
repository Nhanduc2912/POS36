using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using POS36.Api.Data;
using POS36.Api.DTOs;
using POS36.Api.Models;

using System.Security.Claims;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SanPhamController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SanPhamController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCuaHangId()
        {
            var claim = User.FindFirst("CuaHangId");
            if (claim == null) throw new UnauthorizedAccessException("Token không hợp lệ");
            return int.Parse(claim.Value);
        }

        // 1. LẤY DANH SÁCH MÓN ĂN (Kèm theo tên danh mục cho dễ nhìn)
        [HttpGet("danh-sach")]
        public async Task<IActionResult> GetDanhSach([FromQuery] int chiNhanhId, [FromQuery] int? danhMucId)
        {
            // Lấy ID cửa hàng từ Token
            var claim = User.FindFirst("CuaHangId");
            if (claim == null) return Unauthorized();
            int cuaHangId = int.Parse(claim.Value);

            // Lọc sản phẩm theo Cửa hàng và Danh mục (nếu có chọn)
            var query = _context.SanPhams.Where(s => s.CuaHangId == cuaHangId);
            if (danhMucId.HasValue && danhMucId.Value > 0)
            {
                query = query.Where(s => s.DanhMucId == danhMucId.Value);
            }

            // Kết hợp với bảng Tồn Kho để lấy số lượng thực tế tại Chi nhánh
            var result = await query.Select(s => new
            {
                s.Id,
                s.TenSanPham,
                s.GiaBan,
                s.TrangThai,
                s.DanhMucId,
                TenDanhMuc = s.DanhMuc.TenDanhMuc,

                HinhAnh = s.HinhAnh, // <--- THÊM ĐÚNG DÒNG NÀY VÀO ĐÂY LÀ XONG
                // Tìm tồn kho của sản phẩm này tại chi nhánh đang chọn, nếu không có thì trả về 0
                TonKho = _context.TonKhos
                            .Where(t => t.SanPhamId == s.Id && t.ChiNhanhId == chiNhanhId)
                            .Select(t => t.SoLuong)
                            .FirstOrDefault()
            }).ToListAsync();

            return Ok(result);
        }


        [HttpPut("toggle-status/{id}")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            int cuaHangId = GetCuaHangId();

            // Tìm sản phẩm đúng ID và đúng Cửa hàng
            var sp = await _context.SanPhams
                .FirstOrDefaultAsync(s => s.Id == id && s.CuaHangId == cuaHangId);

            if (sp == null) return NotFound("Không tìm thấy sản phẩm!");

            // Đảo ngược trạng thái (Đang true thành false, đang false thành true)
            sp.TrangThai = !sp.TrangThai;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã cập nhật trạng thái!", newStatus = sp.TrangThai });
        }
        // 4. SỬA MÓN ĂN
        // 4. SỬA MÓN ĂN (Sửa thành [FromForm] và dùng lại CreateSanPhamDto)
        // BUG #12 FIX: Chỉ ChuCuaHang mới được sửa sản phẩm
        [Authorize(Roles = "ChuCuaHang")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSanPham(int id, [FromForm] CreateSanPhamDto request)
        {
            int cuaHangId = GetCuaHangId();
            var sp = await _context.SanPhams.FirstOrDefaultAsync(s => s.Id == id && s.CuaHangId == cuaHangId);
            if (sp == null) return NotFound("Không tìm thấy sản phẩm!");

            sp.TenSanPham = request.TenSanPham;
            sp.GiaBan = request.GiaBan;
            sp.DanhMucId = request.DanhMucId;

            // Nếu người dùng có chọn ảnh mới thì mới up và đè lên ảnh cũ
            if (request.HinhAnhFile != null)
            {
                sp.HinhAnh = await UploadImageAsync(request.HinhAnhFile);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công!" });
        }

        // BUG #12 FIX: Chỉ ChuCuaHang mới được xóa sản phẩm
        [Authorize(Roles = "ChuCuaHang")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSanPham(int id)
        {
            int cuaHangId = GetCuaHangId();
            var sp = await _context.SanPhams.FirstOrDefaultAsync(s => s.Id == id && s.CuaHangId == cuaHangId);
            if (sp == null) return NotFound("Không tìm thấy sản phẩm!");

            // BUG #9 FIX: Kiểm tra sản phẩm có đang được dùng trong hóa đơn đang phục vụ không
            bool dangDuocGoiMon = await _context.ChiTietHoaDons
                .AnyAsync(ct => ct.SanPhamId == id
                             && ct.HoaDon != null
                             && ct.HoaDon.TrangThai == "Đang phục vụ");
            if (dangDuocGoiMon)
                return BadRequest("Không thể xóa sản phẩm đang có trong hóa đơn chưa thanh toán!");

            // Xóa luôn các bản ghi Tồn Kho liên quan đến sản phẩm này trước (để tránh lỗi khóa ngoại)
            var tonKhos = await _context.TonKhos.Where(t => t.SanPhamId == id).ToListAsync();
            _context.TonKhos.RemoveRange(tonKhos);

            _context.SanPhams.Remove(sp);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa thành công!" });
        }
        // DTO nhận dữ liệu từ Form (Có chứa File ảnh)
        public class CreateSanPhamDto
        {
            public int DanhMucId { get; set; }
            public string TenSanPham { get; set; } = string.Empty;
            public decimal GiaBan { get; set; }
            public IFormFile? HinhAnhFile { get; set; } // Nhận file ảnh từ Vue
        }

        // HÀM HỖ TRỢ LƯU FILE ẢNH VÀO THƯ MỤC wwwroot/images
        private async Task<string?> UploadImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            // Tạo thư mục wwwroot/images nếu chưa có
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            // Đổi tên file để không bị trùng (dùng GUID)
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/images/" + uniqueFileName; // Trả về đường dẫn để lưu vào DB
        }

        // THÊM MỚI SẢN PHẨM (Dùng [FromForm] thay vì [FromBody])
        // BUG #12 FIX: Chỉ ChuCuaHang mới được thêm sản phẩm mới
        [Authorize(Roles = "ChuCuaHang")]
        [HttpPost]
        public async Task<IActionResult> CreateSanPham([FromForm] CreateSanPhamDto request)
        {
            int cuaHangId = GetCuaHangId();

            var checkDanhMuc = await _context.DanhMucs.AnyAsync(d => d.Id == request.DanhMucId && d.CuaHangId == cuaHangId);
            if (!checkDanhMuc) return BadRequest("Danh mục không hợp lệ!");

            // Xử lý lưu ảnh (nếu có)
            string? hinhAnhPath = await UploadImageAsync(request.HinhAnhFile);

            var newSanPham = new SanPham
            {
                CuaHangId = cuaHangId,
                DanhMucId = request.DanhMucId,
                TenSanPham = request.TenSanPham,
                GiaBan = request.GiaBan,
                TrangThai = true,
                HinhAnh = hinhAnhPath // Gắn đường dẫn ảnh vào DB
            };

            _context.SanPhams.Add(newSanPham);
            await _context.SaveChangesAsync();
            Log.Information("📦 Đã thêm sản phẩm mới: {TenSanPham} (Giá: {GiaBan} VND)", request.TenSanPham, request.GiaBan);
            return Ok(new { message = "Thêm thành công!", id = newSanPham.Id });
        }
        // 6. CẬP NHẬT GIÁ BÁN SIÊU TỐC
        // BUG #12 FIX: Chỉ ChuCuaHang mới được cập nhật giá bán
        [Authorize(Roles = "ChuCuaHang")]
        [HttpPut("update-price/{id}")]
        public async Task<IActionResult> UpdatePrice(int id, [FromBody] UpdatePriceDto request)
        {
            int cuaHangId = GetCuaHangId();
            var sp = await _context.SanPhams.FirstOrDefaultAsync(s => s.Id == id && s.CuaHangId == cuaHangId);
            if (sp == null) return NotFound("Không tìm thấy sản phẩm!");

            sp.GiaBan = request.GiaBan;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật giá thành công!" });
        }
    }
}