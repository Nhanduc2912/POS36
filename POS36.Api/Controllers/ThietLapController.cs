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
    [Authorize] // Vẫn phải có thẻ bài JWT mới được vào
    public class ThietLapController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ThietLapController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCuaHangId()
        {
            var claim = User.FindFirst("CuaHangId");
            if (claim == null) throw new UnauthorizedAccessException("Token không hợp lệ");
            return int.Parse(claim.Value);
        }

        // ==========================================
        // 1. THÊM CHI NHÁNH
        // ==========================================
        // BUG #12 FIX: Chỉ ChuCuaHang mới được thêm Chi Nhánh
        [Authorize(Roles = "ChuCuaHang")]
        [HttpPost("chinhanh")]
        public async Task<IActionResult> CreateChiNhanh(ChiNhanhDto request)
        {
            int cuaHangId = GetCuaHangId();
            var newChiNhanh = new ChiNhanh
            {
                CuaHangId = cuaHangId,
                TenChiNhanh = request.TenChiNhanh,
                DiaChi = request.DiaChi
            };

            _context.ChiNhanhs.Add(newChiNhanh);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Thêm Chi Nhánh thành công!", id = newChiNhanh.Id });
        }

        // ==========================================
        // 1.5 LẤY DANH SÁCH CHI NHÁNH CỦA CHỦ QUÁN
        // ==========================================
        [HttpGet("chinhanh")]
        public async Task<IActionResult> GetChiNhanhs()
        {
            int cuaHangId = GetCuaHangId();
            var branches = await _context.ChiNhanhs
                .Where(c => c.CuaHangId == cuaHangId && !c.IsDeleted)
                .Select(c => new { c.Id, c.TenChiNhanh, c.DiaChi })
                .ToListAsync();

            return Ok(branches);
        }

        // XÓA MỀM CHI NHÁNH
        [Authorize(Roles = "ChuCuaHang")]
        [HttpDelete("chinhanh/{id}")]
        public async Task<IActionResult> DeleteChiNhanh(int id)
        {
            int cuaHangId = GetCuaHangId();
            var cn = await _context.ChiNhanhs.FirstOrDefaultAsync(c => c.Id == id && c.CuaHangId == cuaHangId && !c.IsDeleted);
            if (cn == null) return NotFound("Không tìm thấy chi nhánh!");
            cn.IsDeleted = true; cn.NgayXoa = DateTime.Now;
            cn.NguoiXoa = User.FindFirst("TenDangNhap")?.Value ?? User.Identity?.Name;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa chi nhánh thành công!" });
        }

        // KHÔI PHỤC CHI NHÁNH
        [Authorize(Roles = "ChuCuaHang")]
        [HttpPut("chinhanh/{id}/restore")]
        public async Task<IActionResult> RestoreChiNhanh(int id)
        {
            int cuaHangId = GetCuaHangId();
            var cn = await _context.ChiNhanhs.FirstOrDefaultAsync(c => c.Id == id && c.CuaHangId == cuaHangId && c.IsDeleted);
            if (cn == null) return NotFound("Không tìm thấy chi nhánh đã xóa!");
            cn.IsDeleted = false; cn.NgayXoa = null; cn.NguoiXoa = null;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Khôi phục chi nhánh thành công!" });
        }

        // ==========================================
        // 2. THÊM KHU VỰC VÀO CHI NHÁNH
        // ==========================================
        // BUG #12 FIX: Chỉ ChuCuaHang mới được thêm Khu Vực
        [Authorize(Roles = "ChuCuaHang")]
        [HttpPost("khuvuc")]
        public async Task<IActionResult> CreateKhuVuc(KhuVucDto request)
        {
            int cuaHangId = GetCuaHangId();

            // Check xem chi nhánh này có đúng là của ông chủ này không
            var checkChiNhanh = await _context.ChiNhanhs.AnyAsync(c => c.Id == request.ChiNhanhId && c.CuaHangId == cuaHangId && !c.IsDeleted);
            if (!checkChiNhanh) return BadRequest("Chi nhánh không hợp lệ!");

            var newKhuVuc = new KhuVuc
            {
                CuaHangId = cuaHangId,
                ChiNhanhId = request.ChiNhanhId,
                TenKhuVuc = request.TenKhuVuc
            };

            _context.KhuVucs.Add(newKhuVuc);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Thêm Khu Vực thành công!", id = newKhuVuc.Id });
        }

        // ==========================================
        // 3. THÊM BÀN VÀO KHU VỰC
        // ==========================================
        // BUG #12 FIX: Chỉ ChuCuaHang mới được thêm Bàn
        [Authorize(Roles = "ChuCuaHang")]
        [HttpPost("ban")]
        public async Task<IActionResult> CreateBan(BanDto request)
        {
            int cuaHangId = GetCuaHangId();

            var checkKhuVuc = await _context.KhuVucs.AnyAsync(k => k.Id == request.KhuVucId && k.CuaHangId == cuaHangId);
            if (!checkKhuVuc) return BadRequest("Khu vực không hợp lệ!");

            var newBan = new Ban
            {
                CuaHangId = cuaHangId,
                KhuVucId = request.KhuVucId,
                MaBan = request.MaBan,
                TenBan = request.TenBan,
                TrangThai = "Trống" // Mặc định bàn mới tạo là Trống
            };

            _context.Bans.Add(newBan);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Thêm Bàn thành công!", id = newBan.Id });
        }

        // ==========================================
        // 4. LẤY TOÀN BỘ SƠ ĐỒ QUÁN (CHI NHÁNH -> KHU VỰC -> BÀN)
        // ==========================================
        [HttpGet("sodoban/{chiNhanhId}")]
        public async Task<IActionResult> GetSoDoBan(int chiNhanhId)
        {
            int cuaHangId = GetCuaHangId();

            // Kỹ thuật JOIN (Include) nhiều tầng trong Entity Framework
            var soDo = await _context.KhuVucs
                .Include(k => k.Bans) // Lấy luôn danh sách Bàn nằm trong Khu Vực đó
                .Where(k => k.CuaHangId == cuaHangId && k.ChiNhanhId == chiNhanhId)
                .Select(k => new
                {
                    KhuVucId = k.Id,
                    TenKhuVuc = k.TenKhuVuc,
                    DanhSachBan = k.Bans!.Select(b => new
                    {
                        b.Id,
                        b.MaBan,
                        b.TenBan,
                        b.TrangThai
                    }).ToList()
                })
                .ToListAsync();

            return Ok(soDo);
        }
        // ==========================================
        // 5. LẤY CẤU HÌNH (NGÂN HÀNG, MẪU IN, V.V...)
        // ==========================================
        [HttpGet("{maThietLap}")]
        public async Task<IActionResult> GetThietLap(string maThietLap)
        {
            int cuaHangId = GetCuaHangId();
            var thietLap = await _context.ThietLaps
                .FirstOrDefaultAsync(t => t.CuaHangId == cuaHangId && t.MaThietLap == maThietLap);

            if (thietLap == null) return Ok(new { duLieu = "" }); // Nếu chưa có cấu hình thì trả về rỗng

            return Ok(new { duLieu = thietLap.DuLieu });
        }

        // ==========================================
        // 6. LƯU CẤU HÌNH (THÊM MỚI HOẶC CẬP NHẬT)
        // ==========================================
        // BUG #12 FIX: Chỉ ChuCuaHang mới được lưu thiết lập hệ thống
        [Authorize(Roles = "ChuCuaHang")]
        [HttpPost]
        public async Task<IActionResult> SaveThietLap([FromBody] ThietLapDataDto request)
        {
            int cuaHangId = GetCuaHangId();

            // Tìm xem cửa hàng này đã lưu cấu hình này (VD: BankConfig) bao giờ chưa
            var thietLap = await _context.ThietLaps
                .FirstOrDefaultAsync(t => t.CuaHangId == cuaHangId && t.MaThietLap == request.MaThietLap);

            if (thietLap == null)
            {
                // Chưa có thì tạo mới
                _context.ThietLaps.Add(new ThietLap
                {
                    CuaHangId = cuaHangId,
                    MaThietLap = request.MaThietLap,
                    DuLieu = request.DuLieu
                });
            }
            else
            {
                // Có rồi thì ghi đè dữ liệu mới
                thietLap.DuLieu = request.DuLieu;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Lưu thiết lập thành công" });
        }

        // ==========================================
        // 7. LẤY NHIỀU THIẾT LẬP CÙNG LÚC (batch)
        // ==========================================
        [HttpGet("batch")]
        public async Task<IActionResult> GetBatch([FromQuery] string keys)
        {
            int cuaHangId = GetCuaHangId();
            var keyList = keys.Split(',', StringSplitOptions.RemoveEmptyEntries);

            var result = await _context.ThietLaps
                .Where(t => t.CuaHangId == cuaHangId && keyList.Contains(t.MaThietLap))
                .ToDictionaryAsync(t => t.MaThietLap, t => t.DuLieu);

            // Đảm bảo trả về tất cả keys, kể cả chưa có (giá trị rỗng)
            foreach (var k in keyList)
                if (!result.ContainsKey(k)) result[k] = "";

            return Ok(result);
        }

        // ==========================================
        // 8. LƯU NHIỀU THIẾT LẬP CÙNG LÚC (batch)
        // ==========================================
        [Authorize(Roles = "ChuCuaHang")]
        [HttpPost("batch")]
        public async Task<IActionResult> SaveBatch([FromBody] Dictionary<string, string> data)
        {
            if (data == null || !data.Any()) return BadRequest("Không có dữ liệu!");
            int cuaHangId = GetCuaHangId();

            foreach (var (key, value) in data)
            {
                var tl = await _context.ThietLaps
                    .FirstOrDefaultAsync(t => t.CuaHangId == cuaHangId && t.MaThietLap == key);

                if (tl == null)
                    _context.ThietLaps.Add(new ThietLap { CuaHangId = cuaHangId, MaThietLap = key, DuLieu = value });
                else
                    tl.DuLieu = value;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Đã lưu {data.Count} thiết lập!" });
        }

        // ==========================================
        // 9. LẤY THÔNG TIN CỬA HÀNG (profile)
        // ==========================================
        [HttpGet("store-info")]
        public async Task<IActionResult> GetStoreInfo()
        {
            int cuaHangId = GetCuaHangId();
            var ch = await _context.CuaHangs.FindAsync(cuaHangId);
            if (ch == null) return NotFound();
            return Ok(new
            {
                ch.TenCuaHang, ch.SoDienThoai, ch.Email,
                ch.DiaChi, ch.LogoUrl, ch.TrangThai,
                ch.NgayHetHan, ch.GoiDichVu
            });
        }

        // ==========================================
        // 10. CẬP NHẬT THÔNG TIN CỬA HÀNG
        // ==========================================
        [Authorize(Roles = "ChuCuaHang")]
        [HttpPut("store-info")]
        public async Task<IActionResult> UpdateStoreInfo([FromBody] UpdateStoreInfoDto dto)
        {
            int cuaHangId = GetCuaHangId();
            var ch = await _context.CuaHangs.FindAsync(cuaHangId);
            if (ch == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.TenCuaHang)) ch.TenCuaHang = dto.TenCuaHang.Trim();
            if (dto.Email != null) ch.Email = dto.Email.Trim();
            if (dto.DiaChi != null) ch.DiaChi = dto.DiaChi.Trim();
            if (dto.LogoUrl != null) ch.LogoUrl = dto.LogoUrl.Trim();

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thông tin cửa hàng thành công!" });
        }
    }
}

// DTO mới
public class UpdateStoreInfoDto
{
    public string? TenCuaHang { get; set; }
    public string? Email { get; set; }
    public string? DiaChi { get; set; }
    public string? LogoUrl { get; set; }
}