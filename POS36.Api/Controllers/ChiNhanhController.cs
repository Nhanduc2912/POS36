using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using POS36.Api.Data;
using POS36.Api.Models;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Bắt buộc phải có Token mới cho gọi
    public class ChiNhanhController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ChiNhanhController(AppDbContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> GetDanhSach()
        {
            // Tự động lấy ID Cửa hàng của người đang đăng nhập từ Token
            var cuaHangId = int.Parse(User.FindFirst("CuaHangId")!.Value);
            var query = _context.ChiNhanhs.Where(c => c.CuaHangId == cuaHangId);

            var branchClaim = User.FindFirst("ChiNhanhId");
            if (branchClaim != null)
            {
                int userBranchId = int.Parse(branchClaim.Value);
                query = query.Where(c => c.Id == userBranchId);
            }

            var ds = await query.ToListAsync();
            return Ok(ds);
        }

        public class CreateChiNhanhDto { public string TenChiNhanh { get; set; } = string.Empty; }

        [HttpPost]
        [Authorize(Roles = "ChuCuaHang,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateChiNhanhDto request)
        {
            var cuaHangId = int.Parse(User.FindFirst("CuaHangId")!.Value);
            var newBranch = new ChiNhanh { CuaHangId = cuaHangId, TenChiNhanh = request.TenChiNhanh };
            _context.ChiNhanhs.Add(newBranch);
            await _context.SaveChangesAsync();
            return Ok(newBranch);
        }
    }
}