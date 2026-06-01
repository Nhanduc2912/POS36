using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using POS36.Api.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ChuCuaHang,Admin,QuanLy")]
    public class NhatKyController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NhatKyController(AppDbContext context)
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
        public async Task<IActionResult> GetNhatKy(
            [FromQuery] int chiNhanhId,
            [FromQuery] string? startDate = null,
            [FromQuery] string? endDate = null,
            [FromQuery] string? search = null)
        {
            try
            {
                int cuaHangId = GetCuaHangId();

                var dbQuery = _context.NhatKyHoatDongs.AsQueryable();

                if (chiNhanhId > 0)
                {
                    dbQuery = dbQuery.Where(n => n.ChiNhanhId == chiNhanhId);
                }

                // Lấy toàn bộ bản ghi theo bộ lọc chi nhánh để xử lý khoảng thời gian local/UTC
                var list = await dbQuery.OrderByDescending(n => n.ThoiGian).ToListAsync();

                // Lọc theo khoảng ngày giao dịch giao diện gửi lên
                if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var sd))
                {
                    var start = sd.Date;
                    list = list.Where(n => n.ThoiGian.Date >= start).ToList();
                }

                if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var ed))
                {
                    var end = ed.Date;
                    list = list.Where(n => n.ThoiGian.Date <= end).ToList();
                }

                // Lọc tìm kiếm theo từ khóa
                if (!string.IsNullOrEmpty(search))
                {
                    var kw = search.ToLower().Trim();
                    list = list.Where(n => 
                        n.NguoiThucHien.ToLower().Contains(kw) || 
                        n.HanhDong.ToLower().Contains(kw) || 
                        n.MoTa.ToLower().Contains(kw) ||
                        (n.VaiTro != null && n.VaiTro.ToLower().Contains(kw))
                    ).ToList();
                }

                // Trả về tối đa 500 dòng mới nhất để tối ưu hiệu năng
                var result = list.Take(500).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi server khi lấy nhật ký hoạt động: " + ex.Message);
            }
        }
    }
}
