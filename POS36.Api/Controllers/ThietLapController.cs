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
        // 2. THÊM KHU VỰC VÀO CHI NHÁNH
        // ==========================================
        [HttpPost("khuvuc")]
        public async Task<IActionResult> CreateKhuVuc(KhuVucDto request)
        {
            int cuaHangId = GetCuaHangId();

            // Check xem chi nhánh này có đúng là của ông chủ này không
            var checkChiNhanh = await _context.ChiNhanhs.AnyAsync(c => c.Id == request.ChiNhanhId && c.CuaHangId == cuaHangId);
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
    }
}