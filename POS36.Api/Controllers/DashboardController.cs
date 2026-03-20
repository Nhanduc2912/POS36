using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using POS36.Api.Data;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;
        public DashboardController(AppDbContext context) { _context = context; }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] int? chiNhanhId) // Nhận ID chi nhánh từ Frontend
        {
            var cuaHangId = int.Parse(User.FindFirst("CuaHangId")!.Value);

            // Kiểm tra xem chi nhánh này có đúng là của cửa hàng đang đăng nhập không (Bảo mật)
            var validBranchIds = await _context.ChiNhanhs
                                    .Where(c => c.CuaHangId == cuaHangId)
                                    .Select(c => c.Id)
                                    .ToListAsync();

            // Nếu frontend truyền chiNhanhId lên, thì chỉ lọc theo chi nhánh đó
            List<int> targetBranchIds = new List<int>();
            if (chiNhanhId.HasValue && validBranchIds.Contains(chiNhanhId.Value))
            {
                targetBranchIds.Add(chiNhanhId.Value);
            }
            else
            {
                // Nếu chưa truyền hoặc truyền sai, mặc định trả về 0 để an toàn
                return Ok(new
                {
                    summary = new { tongDonHang = 0, doanhThu = 0, donHuy = 0, banDangDung = 0, tongBan = 0 },
                    chart = new { labels = new List<string>(), data = new List<int>() }
                });
            }

            // ĐẾM SỐ LƯỢNG BÀN THEO ĐÚNG CHI NHÁNH ĐÓ
            var tongBan = await _context.Bans
                                .Include(b => b.KhuVuc)
                                .Where(b => targetBranchIds.Contains(b.KhuVuc.ChiNhanhId))
                                .CountAsync();

            var banDangDung = await _context.Bans
                                    .Include(b => b.KhuVuc)
                                    .Where(b => targetBranchIds.Contains(b.KhuVuc.ChiNhanhId) && b.TrangThai == "Đang sử dụng")
                                    .CountAsync();

            return Ok(new
            {
                summary = new { tongDonHang = 0, doanhThu = 0, donHuy = 0, banDangDung = banDangDung, tongBan = tongBan },
                chart = new { labels = new List<string>(), data = new List<int>() }
            });
        }
    }
}