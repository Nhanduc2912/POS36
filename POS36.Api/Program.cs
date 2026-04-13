using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using POS36.Api.Data;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace POS36.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // ========================================================
            // 1. MÀN HÌNH KHỞI ĐỘNG (BOOT SCREEN) CHUẨN ENTERPRISE
            // ========================================================
            try { Console.Clear(); } catch { }
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(@"
    ██████╗  ██████╗  ██████╗ ██████╗  ██████╗ 
    ██╔══██╗██╔═══██╗██╔════╝ ╚════██╗ ██╔════╝ 
    ██████╔╝██║   ██║╚█████╗   █████╔╝ ███████╗ 
    ██╔═══╝ ██║   ██║ ╚═══██╗  ╚═══██╗ ██╔═══██╗
    ██║     ╚██████╔╝██████╔╝ ██████╔╝ ╚██████╔╝
    ╚═╝      ╚═════╝ ╚═════╝  ╚═════╝   ╚═════╝ ");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("    =========================================================");
            Console.WriteLine("    🚀 HỆ THỐNG ĐIỀU HÀNH LÕI - POS36 CULINARY KINETIC API 🚀");
            Console.WriteLine("    =========================================================");
            Console.ResetColor();

            // Hiển thị thông số cấu hình Server
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"    [+] Khởi động lúc : {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            Console.WriteLine($"    [+] Máy chủ       : {Environment.MachineName}");
            Console.WriteLine($"    [+] Nền tảng OS   : {RuntimeInformation.OSDescription}");
            Console.WriteLine($"    [+] Core Engine   : {RuntimeInformation.FrameworkDescription}");
            Console.WriteLine($"    [+] RAM cấp phát  : {Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024} MB");
            Console.WriteLine("    =========================================================\n");
            Console.ResetColor();

            // ========================================================
            // 2. CẤU HÌNH SERILOG THEME HACKER
            // ========================================================
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                .MinimumLevel.Override("System", LogEventLevel.Error)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Error)
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                    theme: AnsiConsoleTheme.Code)
                .CreateLogger();

            try
            {
                var builder = WebApplication.CreateBuilder(args);
                builder.Host.UseSerilog();

                // Kết nối DB & JWT & Swagger & CORS
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

                builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = builder.Configuration["Jwt:Issuer"],
                            ValidAudience = builder.Configuration["Jwt:Audience"],
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                        };
                    });

                builder.Services.AddControllers();
                builder.Services.AddSignalR();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "POS36 API", Version = "v1" });
                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = "Nhập 'Bearer' [khoảng trắng] và dán Token.",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer"
                    });
                    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                                Scheme = "oauth2", Name = "Bearer", In = ParameterLocation.Header,
                            },
                            new List<string>()
                        }
                    });
                });

                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowVueApp", builder =>
                    {
                        builder.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                    });
                });

                var app = builder.Build();

                // ========================================================
                // 3. MIDDLEWARE GIÁM SÁT REQUEST (SIÊU CẤP ĐẲNG CẤP)
                // ========================================================
                app.Use(async (context, next) =>
                {
                    var method = context.Request.Method;

                    // 🛑 Bỏ qua log rác OPTIONS
                    if (method == "OPTIONS")
                    {
                        await next();
                        return;
                    }

                    var sw = Stopwatch.StartNew();
                    await next();
                    sw.Stop();

                    var statusCode = context.Response.StatusCode;
                    var path = context.Request.Path;

                    // 1. Phân loại Icon Trạng thái
                    string icon = statusCode switch
                    {
                        >= 200 and < 300 => "✅",
                        >= 300 and < 400 => "🔀",
                        >= 400 and < 500 => "⚠️",
                        _ => "❌"
                    };

                    // 2. Đánh giá tốc độ phản hồi (Performance)
                    string speedIndicator = sw.ElapsedMilliseconds switch
                    {
                        < 100 => "⚡", // Siêu tốc
                        < 500 => "🚀", // Nhanh
                        < 2000 => "🐢", // Chậm
                        _ => "🔥" // Cực chậm (báo động)
                    };

                    // 3. Bắt IP Khách
                    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                    if (ip == "::1" || ip == "127.0.0.1") ip = "Local";

                    // 4. Nhận diện thiết bị
                    var userAgent = context.Request.Headers["User-Agent"].ToString();
                    string device = userAgent.Contains("Mobile") ? "📱 Mob " : "💻 Web ";

                    // 5. Bắt Danh tính người dùng (Nếu có Token)
                    string user = "Khách lạ";
                    if (context.User.Identity?.IsAuthenticated == true)
                    {
                        // Giả sử sếp lưu Username vào Claim, móc nó ra
                        user = context.User.Identity.Name ?? "Tài khoản";
                    }

                    // Format log thành một bảng siêu đẹp và thẳng hàng
                    string logMessage = $"{icon} {statusCode} | {speedIndicator} {sw.ElapsedMilliseconds,5}ms | 👤 {user,-10} | 🌐 {ip,-15} | {device} | {method,-6} {path}";

                    // Nếu lỗi (4xx, 5xx) thì in màu đỏ, nếu thành công in bình thường
                    if (statusCode >= 400)
                        Log.Warning(logMessage);
                    else
                        Log.Information(logMessage);
                });

                // ========================================================

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseStaticFiles();
                app.UseCors("AllowVueApp");
                app.UseAuthentication();
                app.UseAuthorization();

                app.MapHub<POS36.Api.Hubs.KitchenHub>("/kitchenHub");
                app.MapControllers();

                Log.Information("🟢 BỘ MÁY ĐÃ SẴN SÀNG! Đang lắng nghe luồng dữ liệu tại cổng 5198...");
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "❌ HỆ THỐNG CORE BỊ SẬP!");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}