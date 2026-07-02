using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ChuCuaHang,Admin,QuanLy")]
    public class ThongBaoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ThongBaoController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCuaHangId()
        {
            var claim = User.FindFirst("CuaHangId");
            if (claim == null) throw new UnauthorizedAccessException("Token không hợp lệ");
            return int.Parse(claim.Value);
        }

        [HttpGet]
        public async Task<IActionResult> GetThongBaoHeThong()
        {
            try
            {
                int cuaHangId = GetCuaHangId();

                var list = await _context.ThongBaoHeThongs
                    .Where(t => t.CuaHangId == null || t.CuaHangId == cuaHangId)
                    .OrderByDescending(t => t.NgayTao)
                    .Take(50)
                    .ToListAsync();

                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi server khi tải thông báo: " + ex.Message);
            }
        }
    }
}
