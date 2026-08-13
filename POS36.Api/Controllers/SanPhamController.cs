using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using POS36.Api.Data;
using POS36.Api.DTOs;
using POS36.Api.Models;

using System.Security.Claims;

using POS36.Api.Services;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SanPhamController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ICloudStorageService _cloudStorage;

        public SanPhamController(AppDbContext context, ICloudStorageService cloudStorage)
        {
            _context = context;
            _cloudStorage = cloudStorage;
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

            var branchClaim = User.FindFirst("ChiNhanhId");
            if (branchClaim != null)
            {
                int userBranchId = int.Parse(branchClaim.Value);
                if (chiNhanhId > 0 && chiNhanhId != userBranchId)
                {
                    return StatusCode(403, "Bạn không có quyền truy cập dữ liệu của chi nhánh khác!");
                }
                chiNhanhId = userBranchId;
            }

            // Lọc sản phẩm theo Cửa hàng và Danh mục (nếu có chọn)
            var query = _context.SanPhams.Where(s => s.CuaHangId == cuaHangId);
            if (danhMucId.HasValue && danhMucId.Value > 0)
            {
                query = query.Where(s => s.DanhMucId == danhMucId.Value);
            }

            // Kết hợp với bảng Tồn Kho để lấy số lượng thực tế tại Chi nhánh
            var sanPhams = await query.Select(s => new
            {
                s.Id,
                s.TenSanPham,
                s.GiaBan,
                s.TrangThai,
                s.DanhMucId,
                TenDanhMuc = s.DanhMuc != null ? s.DanhMuc.TenDanhMuc : "Khác",
                HinhAnh = s.HinhAnh,
                NgưỡngCanhBao = s.NgưỡngCanhBao // FEAT-2
            }).ToListAsync();

            // Tính giá vốn server-side: Σ(Định lượng NVL × Giá vốn MAC của NVL đó)
            var allDinhLuong = await _context.DinhLuongs
                .Include(d => d.NguyenVatLieu)
                .Where(d => sanPhams.Select(sp => sp.Id).Contains(d.SanPhamId))
                .ToListAsync();

            var result = sanPhams.Select(s => new
            {
                s.Id,
                s.TenSanPham,
                s.GiaBan,
                s.TrangThai,
                s.DanhMucId,
                s.TenDanhMuc,
                s.HinhAnh,
                GiaVon = Math.Round((double)allDinhLuong
                    .Where(d => d.SanPhamId == s.Id && d.NguyenVatLieu != null)
                    .Sum(d => d.SoLuong * d.NguyenVatLieu!.GiaVonHienTai), 0),
                CoDinhLuong = allDinhLuong.Any(d => d.SanPhamId == s.Id)
            }).ToList();

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

            await _context.LogHoatDongAsync(int.Parse(User.FindFirst("ChiNhanhId")?.Value ?? "0"), "Thực đơn", $"{(sp.TrangThai ? "Kích hoạt" : "Ngừng kích hoạt")} sản phẩm '{sp.TenSanPham}'");

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

            // Xác thực Danh mục có thuộc về cửa hàng này hay không để chống IDOR
            var checkDanhMuc = await _context.DanhMucs.AnyAsync(d => d.Id == request.DanhMucId && d.CuaHangId == cuaHangId);
            if (!checkDanhMuc) return BadRequest("Danh mục không hợp lệ hoặc không thuộc cửa hàng của bạn!");

            sp.TenSanPham = request.TenSanPham;
            sp.GiaBan = request.GiaBan;
            sp.DanhMucId = request.DanhMucId;
            if (request.NgưỡngCanhBao > 0) sp.NgưỡngCanhBao = request.NgưỡngCanhBao; // FEAT-2

            // Nếu người dùng có chọn ảnh mới thì mới up và đè lên ảnh cũ
            if (request.HinhAnhFile != null)
            {
                sp.HinhAnh = await UploadImageAsync(request.HinhAnhFile);
            }

            await _context.SaveChangesAsync();

            await _context.LogHoatDongAsync(int.Parse(User.FindFirst("ChiNhanhId")?.Value ?? "0"), "Thực đơn", $"Cập nhật sản phẩm '{sp.TenSanPham}'. Giá bán: {sp.GiaBan:N0}đ");

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

            var dinhLuongs = await _context.DinhLuongs.Where(t => t.SanPhamId == id).ToListAsync();
            _context.DinhLuongs.RemoveRange(dinhLuongs);

            _context.SanPhams.Remove(sp);
            await _context.SaveChangesAsync();

            await _context.LogHoatDongAsync(int.Parse(User.FindFirst("ChiNhanhId")?.Value ?? "0"), "Thực đơn", $"Xóa sản phẩm '{sp.TenSanPham}'");

            return Ok(new { message = "Xóa thành công!" });
        }
        // DTO nhận dữ liệu từ Form (Có chứa File ảnh)
        public class CreateSanPhamDto
        {
            public int DanhMucId { get; set; }
            public string TenSanPham { get; set; } = string.Empty;
            public decimal GiaBan { get; set; }
            public IFormFile? HinhAnhFile { get; set; } // Nhận file ảnh từ Vue
            public int NgưỡngCanhBao { get; set; } = 5; // FEAT-2
        }

        // HÀM HỖ TRỢ UPLOAD ẢNH (GỌI CLOUD STORAGE SERVICE)
        private async Task<string?> UploadImageAsync(IFormFile? file)
        {
            return await _cloudStorage.UploadImageAsync(file, "san-pham");
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
                HinhAnh = hinhAnhPath,
                NgưỡngCanhBao = request.NgưỡngCanhBao > 0 ? request.NgưỡngCanhBao : 5 // FEAT-2
            };

            _context.SanPhams.Add(newSanPham);
            await _context.SaveChangesAsync();

            await _context.LogHoatDongAsync(int.Parse(User.FindFirst("ChiNhanhId")?.Value ?? "0"), "Thực đơn", $"Thêm sản phẩm mới '{newSanPham.TenSanPham}' với giá bán {newSanPham.GiaBan:N0}đ");

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

            await _context.LogHoatDongAsync(int.Parse(User.FindFirst("ChiNhanhId")?.Value ?? "0"), "Thực đơn", $"Cập nhật nhanh giá bán sản phẩm '{sp.TenSanPham}' thành {sp.GiaBan:N0}đ");

            return Ok(new { message = "Cập nhật giá thành công!" });
        }

        // ==========================================
        // QUẢN LÝ ĐỊNH LƯỢNG (RECIPE) CHO SẢN PHẨM
        // ==========================================
        [HttpGet("{id}/dinhluong")]
        public async Task<IActionResult> GetDinhLuong(int id)
        {
            int cuaHangId = GetCuaHangId();
            var checkSp = await _context.SanPhams.FirstOrDefaultAsync(s => s.Id == id && s.CuaHangId == cuaHangId);
            if (checkSp == null) return NotFound("Sản phẩm không tồn tại!");

            var data = await _context.DinhLuongs
                .Include(d => d.NguyenVatLieu)
                .Where(d => d.SanPhamId == id)
                .Select(d => new {
                    d.Id, 
                    d.NguyenVatLieuId, 
                    d.SoLuong, 
                    TenNguyenVatLieu = d.NguyenVatLieu != null ? d.NguyenVatLieu.TenNguyenVatLieu : "",
                    DonViTinh = d.NguyenVatLieu != null ? d.NguyenVatLieu.DonViTinh : ""
                }).ToListAsync();

            return Ok(data);
        }

        [Authorize(Roles = "ChuCuaHang")]
        [HttpPost("{id}/dinhluong")]
        public async Task<IActionResult> UpdateDinhLuong(int id, [FromBody] List<DinhLuongDto> requests)
        {
            int cuaHangId = GetCuaHangId();
            var sp = await _context.SanPhams.FirstOrDefaultAsync(s => s.Id == id && s.CuaHangId == cuaHangId);
            if (sp == null) return NotFound("Sản phẩm không tồn tại!");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Xóa toàn bộ định lượng cũ
                var oldList = await _context.DinhLuongs.Where(d => d.SanPhamId == id).ToListAsync();
                _context.DinhLuongs.RemoveRange(oldList);

                // Thêm mới
                if (requests != null && requests.Any())
                {
                    var newList = requests.Select(r => new DinhLuong {
                        SanPhamId = id,
                        NguyenVatLieuId = r.NguyenVatLieuId,
                        SoLuong = r.SoLuong
                    });
                    _context.DinhLuongs.AddRange(newList);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                await _context.LogHoatDongAsync(int.Parse(User.FindFirst("ChiNhanhId")?.Value ?? "0"), "Thực đơn", $"Cập nhật công thức/định lượng cho sản phẩm '{sp.TenSanPham}'");
                return Ok(new { message = "Cập nhật định lượng thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi server: " + ex.Message);
            }
        }
    }
}