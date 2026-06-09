using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using System.Text.Json;

namespace POS36.Api.Middleware
{
    /// <summary>
    /// Middleware kiểm tra trạng thái subscription sau khi JWT đã xác thực.
    /// Nếu cửa hàng ở trạng thái BiKhoa hoặc ChiDoc, chặn các thao tác ghi.
    /// </summary>
    public class SubscriptionMiddleware
    {
        private readonly RequestDelegate _next;

        // Routes được phép dù hết hạn (read-only access)
        private static readonly HashSet<string> _whitelist = new(StringComparer.OrdinalIgnoreCase)
        {
            "/api/auth/login",
            "/api/auth/refresh",
            "/api/thietlap/store-info",
            "/api/thietlap/subscription",
            "/api/thietlap/chinhanh",
            "/api/subscription",
            "/api/dashboard/overview",    // Xem Dashboard
            "/api/dashboard/summary",
            "/api/hoadon",                // Xem hóa đơn (GET only)
        };

        public SubscriptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
        {
            // Chỉ check với API calls có token
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                await _next(context);
                return;
            }

            // Bỏ qua nếu chưa auth hoặc là SuperAdmin
            var user = context.User;
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                await _next(context);
                return;
            }

            var role = user.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value
                    ?? user.FindFirst("VaiTro")?.Value;

            // SuperAdmin không bị chặn
            if (role == "SuperAdmin")
            {
                await _next(context);
                return;
            }

            // Lấy CuaHangId từ claim
            var cuaHangIdClaim = user.FindFirst("CuaHangId")?.Value;
            if (!int.TryParse(cuaHangIdClaim, out int cuaHangId))
            {
                await _next(context);
                return;
            }

            // FIX-4: Lấy cả trạng thái cửa hàng VÀ IsActive của tài khoản trong 1 query
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Lấy userId từ claim để check IsActive
            var userIdClaim = user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
                           ?? user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userIdClaim, out int taiKhoanId))
            {
                var isActive = await db.TaiKhoans
                    .Where(t => t.Id == taiKhoanId)
                    .Select(t => t.IsActive)
                    .FirstOrDefaultAsync();

                if (!isActive)
                {
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
                    {
                        code = "ACCOUNT_DISABLED",
                        message = "Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ quản lý!"
                    }));
                    return;
                }
            }

            var trangThai = await db.CuaHangs
                .Where(c => c.Id == cuaHangId)
                .Select(c => c.TrangThai)
                .FirstOrDefaultAsync();

            var path = context.Request.Path.Value ?? "";
            var method = context.Request.Method;

            if (trangThai == "BiKhoa")
            {
                // Khóa hoàn toàn — chỉ cho phép whitelist GET
                var isWhitelisted = _whitelist.Any(w => path.StartsWith(w, StringComparison.OrdinalIgnoreCase))
                                    && method == "GET";
                if (!isWhitelisted)
                {
                    context.Response.StatusCode = 403;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        code = "STORE_LOCKED",
                        message = "Cửa hàng của bạn đã bị khóa. Vui lòng liên hệ Super Admin để mở khóa.",
                        trangThai = "BiKhoa"
                    }));
                    return;
                }
            }
            else if (trangThai == "ChiDoc")
            {
                // Chỉ đọc — chặn POST, PUT, DELETE
                if (method != "GET" && !_whitelist.Any(w => path.StartsWith(w, StringComparison.OrdinalIgnoreCase)))
                {
                    context.Response.StatusCode = 403;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        code = "SUBSCRIPTION_EXPIRED",
                        message = "Gói dịch vụ đã hết hạn. Bạn chỉ có thể xem dữ liệu. Vui lòng gia hạn để tiếp tục.",
                        trangThai = "ChiDoc",
                        redirectTo = "/admin/subscription"
                    }));
                    return;
                }
            }

            await _next(context);
        }
    }

    // Extension method để đăng ký middleware gọn hơn
    public static class SubscriptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseSubscriptionGuard(this IApplicationBuilder builder)
            => builder.UseMiddleware<SubscriptionMiddleware>();
    }
}
