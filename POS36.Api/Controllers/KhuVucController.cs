using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using POS36.Api.Models;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KhuVucController : ControllerBase
    {
        private readonly AppDbContext _context;
        public KhuVucController(AppDbContext context) { _context = context; }

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
            var newArea = new KhuVuc { ChiNhanhId = req.ChiNhanhId, TenKhuVuc = req.TenKhuVuc };
            _context.KhuVucs.Add(newArea);
            await _context.SaveChangesAsync();
            return Ok(newArea);
        }
    }
}