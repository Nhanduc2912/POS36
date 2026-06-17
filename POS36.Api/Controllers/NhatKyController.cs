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
            [FromQuery] string? search = null,
            [FromQuery] string? actions = null,
            [FromQuery] string? quickFilter = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                int cuaHangId = GetCuaHangId();

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

                var dbQuery = _context.NhatKyHoatDongs.AsQueryable();

                if (chiNhanhId > 0)
                {
                    dbQuery = dbQuery.Where(n => n.ChiNhanhId == chiNhanhId);
                }

                // Lọc theo khoảng ngày / lọc nhanh
                if (!string.IsNullOrEmpty(quickFilter) && quickFilter != "custom")
                {
                    var today = DateTime.Today;
                    if (quickFilter == "today")
                    {
                        dbQuery = dbQuery.Where(n => n.ThoiGian >= today);
                    }
                    else if (quickFilter == "yesterday")
                    {
                        var yesterday = today.AddDays(-1);
                        dbQuery = dbQuery.Where(n => n.ThoiGian >= yesterday && n.ThoiGian < today);
                    }
                    else if (quickFilter == "3days")
                    {
                        var threeDaysAgo = today.AddDays(-2);
                        dbQuery = dbQuery.Where(n => n.ThoiGian >= threeDaysAgo);
                    }
                    else if (quickFilter == "7days")
                    {
                        var sevenDaysAgo = today.AddDays(-6);
                        dbQuery = dbQuery.Where(n => n.ThoiGian >= sevenDaysAgo);
                    }
                    else if (quickFilter == "30days")
                    {
                        var thirtyDaysAgo = today.AddDays(-29);
                        dbQuery = dbQuery.Where(n => n.ThoiGian >= thirtyDaysAgo);
                    }
                    else if (quickFilter == "90days")
                    {
                        var ninetyDaysAgo = today.AddDays(-89);
                        dbQuery = dbQuery.Where(n => n.ThoiGian >= ninetyDaysAgo);
                    }
                }
                else
                {
                    // Lọc tùy chỉnh theo khoảng ngày
                    if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var sd))
                    {
                        var start = sd.Date;
                        dbQuery = dbQuery.Where(n => n.ThoiGian >= start);
                    }

                    if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var ed))
                    {
                        var end = ed.Date.AddDays(1).AddTicks(-1);
                        dbQuery = dbQuery.Where(n => n.ThoiGian <= end);
                    }
                }

                // Lọc theo nhóm hành động (actions)
                if (!string.IsNullOrEmpty(actions))
                {
                    var actionList = actions.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                           .Select(a => a.Trim().ToLower())
                                           .ToList();
                    dbQuery = dbQuery.Where(n => actionList.Contains(n.HanhDong.ToLower()));
                }

                // Lọc tìm kiếm theo từ khóa
                if (!string.IsNullOrEmpty(search))
                {
                    var kw = search.ToLower().Trim();
                    dbQuery = dbQuery.Where(n => 
                        n.NguoiThucHien.ToLower().Contains(kw) || 
                        n.HanhDong.ToLower().Contains(kw) || 
                        n.MoTa.ToLower().Contains(kw) ||
                        (n.VaiTro != null && n.VaiTro.ToLower().Contains(kw))
                    );
                }

                var totalCount = await dbQuery.CountAsync();

                // Phân trang
                var list = await dbQuery
                    .OrderByDescending(n => n.ThoiGian)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new
                {
                    totalCount = totalCount,
                    items = list,
                    page = page,
                    pageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi server khi lấy nhật ký hoạt động: " + ex.Message);
            }
        }
    }
}
