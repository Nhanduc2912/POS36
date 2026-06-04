using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using POS36.Api.Data;
using POS36.Api.Models;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class KhuVucController : ControllerBase
    {
        private readonly AppDbContext _context;
        public KhuVucController(AppDbContext context) { _context = context; }

        private int GetCuaHangId() => int.Parse(User.FindFirst("CuaHangId")!.Value);

        [HttpGet("chi-nhanh/{chiNhanhId}")]
        public async Task<IActionResult> GetByChiNhanh(int chiNhanhId)
        {
            var ds = await _context.KhuVucs.Where(k => k.ChiNhanhId == chiNhanhId).ToListAsync();
            return Ok(ds);
        }

        public class CreateKhuVucDto { public int ChiNhanhId { get; set; } public string TenKhuVuc { get; set; } = string.Empty; }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateKhuVucDto req)
        {
            int cuaHangId = GetCuaHangId();

            // Xác thực xem Chi nhánh có thuộc về Cửa hàng của user đăng nhập không
            var chiNhanhHopLe = await _context.ChiNhanhs
                .AnyAsync(cn => cn.Id == req.ChiNhanhId && cn.CuaHangId == cuaHangId);
            if (!chiNhanhHopLe)
                return BadRequest("Chi nhánh không thuộc cửa hàng hiện tại.");

            var newArea = new KhuVuc 
            { 
                CuaHangId = cuaHangId, 
                ChiNhanhId = req.ChiNhanhId, 
                TenKhuVuc = req.TenKhuVuc 
            };
            _context.KhuVucs.Add(newArea);
            await _context.SaveChangesAsync();
            return Ok(newArea);
        }
    }
}