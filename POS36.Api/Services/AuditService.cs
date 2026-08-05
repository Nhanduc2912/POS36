using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using POS36.Api.Models;

namespace POS36.Api.Services
{
    /// <summary>
    /// Service ghi nhật ký hệ thống dùng chung cho tất cả controllers.
    /// Inject qua DI: services.AddScoped&lt;IAuditService, AuditService&gt;();
    /// </summary>
    public interface IAuditService
    {
        Task GhiLog(string hanhDong, string moTa, string? urlLienQuan = null, string? chiTietJson = null);
    }

    public class AuditService : IAuditService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task GhiLog(string hanhDong, string moTa, string? urlLienQuan = null, string? chiTietJson = null)
        {
            try
            {
                var http = _httpContextAccessor.HttpContext;
                string? nguoiThucHien = http?.User?.FindFirst("TenDangNhap")?.Value
                                     ?? http?.User?.Identity?.Name
                                     ?? "Hệ thống";
                string? ip = http?.Connection?.RemoteIpAddress?.ToString();

                _context.NhatKyHeThangs.Add(new NhatKyHeThong
                {
                    HanhDong = hanhDong,
                    MoTa = moTa,
                    NguoiThucHien = nguoiThucHien,
                    IpAddress = ip,
                    UrlLienQuan = urlLienQuan,
                    ChiTietJson = chiTietJson,
                    ThoiGian = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Không để lỗi ghi log làm hỏng luồng chính
            }
        }
    }
}
