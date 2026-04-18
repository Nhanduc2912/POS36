using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using System.Linq;
using System.Threading.Tasks;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ThuChiController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ThuChiController(AppDbContext context) { _context = context; }
        // BUG #8 FIX: Không fallback về "1" — phải throw exception nếu token không hợp lệ
        private int GetCuaHangId()
        {
            var claim = User.FindFirst("CuaHangId");
            if (claim == null) throw new UnauthorizedAccessException("Token không hợp lệ");
            return int.Parse(claim.Value);
        }

        // BUG #12 FIX: Chỉ ChuCuaHang mới được xem sổ quỹ (bảo mật dữ liệu tài chính)
        [Authorize(Roles = "ChuCuaHang")]
        [HttpGet("danh-sach")]
        public async Task<IActionResult> GetDanhSach([FromQuery] int chiNhanhId)
        {
            int cuaHangId = GetCuaHangId();
            var query = _context.PhieuThuChis.Where(p => p.CuaHangId == cuaHangId);

            if (chiNhanhId > 0) query = query.Where(p => p.ChiNhanhId == chiNhanhId);

            var danhSach = await query.OrderByDescending(p => p.NgayGiaoDich).ToListAsync();

            // Tính toán 4 thẻ tổng quát (Giả định Đầu kỳ = 0 để đơn giản hóa lúc này)
            double tongThu = danhSach.Where(p => p.LoaiPhieu == "Thu").Sum(p => p.GiaTri);
            double tongChi = danhSach.Where(p => p.LoaiPhieu == "Chi").Sum(p => p.GiaTri);

            return Ok(new
            {
                ThongKe = new
                {
                    DauKy = 0,
                    TongThu = tongThu,
                    TongChi = tongChi,
                    TonQuy = tongThu - tongChi
                },
                DanhSach = danhSach
            });
        }
    }
}