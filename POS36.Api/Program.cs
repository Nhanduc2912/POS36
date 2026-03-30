using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using POS36.Api.Data;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System.Text;

namespace POS36.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // ========================================================
            // 1. VẼ LOGO POS36 SIÊU NGẦU (ASCII ART) Ở GIỮA MÀN HÌNH
            // ========================================================
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine();
            Console.WriteLine(@"        ██████╗  ██████╗  ██████╗ ██████╗   ██████╗ ");
            Console.WriteLine(@"        ██╔══██╗██╔═══██╗██╔════╝ ╚════██╗ ██╔════╝ ");
            Console.WriteLine(@"        ██████╔╝██║   ██║╚█████╗   █████╔╝ ███████╗ ");
            Console.WriteLine(@"        ██╔═══╝ ██║   ██║ ╚═══██╗  ╚═══██╗ ██╔═══██╗");
            Console.WriteLine(@"        ██║     ╚██████╔╝██████╔╝ ██████╔╝ ╚██████╔╝");
            Console.WriteLine(@"        ╚═╝      ╚═════╝ ╚═════╝  ╚═════╝   ╚═════╝ ");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"        ===========================================");
            Console.WriteLine(@"        🚀 NỀN TẢNG QUẢN LÝ NHÀ HÀNG - POS36 API 🚀");
            Console.WriteLine(@"        ===========================================");
            Console.WriteLine();
            Console.ResetColor();

            // ========================================================
            // 2. CẤU HÌNH SERILOG (ĐÃ KHÓA MÕM 100% RÁC HỆ THỐNG)
            // ========================================================
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Error) // Chỉ báo lỗi, cấm báo lằng nhằng
                .MinimumLevel.Override("System", LogEventLevel.Error)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Warning) // Tắt dòng 'Now listening on...'
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Error) // TẮT SẠCH CHỮ VÀNG SQL DECIMAL
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                    theme: AnsiConsoleTheme.Code)
                .CreateLogger();

            try
            {
                Log.Information("⏳ Đang nạp cấu hình hệ thống...");
                var builder = WebApplication.CreateBuilder(args);

                // Kích hoạt Serilog thay thế Logger mặc định
                builder.Host.UseSerilog();

                // 3. Thêm kết nối Database
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

                // 4. Cấu hình JWT
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

                // 5. Cấu hình Swagger 
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "POS36 API", Version = "v1" });

                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header sử dụng scheme Bearer. \r\n\r\n Nhập 'Bearer' [khoảng trắng] và dán Token của bạn vào.\r\n\r\nVí dụ: 'Bearer eyJhbGci...'",
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
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                },
                                Scheme = "oauth2",
                                Name = "Bearer",
                                In = ParameterLocation.Header,
                            },
                            new List<string>()
                        }
                    });
                });

                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowVueApp",
                        builder =>
                        {
                            builder.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
                                   .AllowAnyHeader()
                                   .AllowAnyMethod()
                                   .AllowCredentials();
                        });
                });

                var app = builder.Build();

                // ========================================================
                // 6. TÍNH NĂNG THEO DÕI REQUEST TỪ CLIENT (BẮT IP)
                // ========================================================
                app.UseSerilogRequestLogging(options =>
                {
                    // Format cực đẹp: [IP] Phương thức - Đường dẫn => Trạng thái (Thời gian)
                    options.MessageTemplate = "🌐 [IP: {ClientIp}] {RequestMethod} {RequestPath} => Mã {StatusCode} ({Elapsed:0.0} ms)";
                    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                    {
                        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                        if (ip == "::1" || ip == "127.0.0.1") ip = "Localhost";
                        diagnosticContext.Set("ClientIp", ip);
                    };
                });

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                // --- KHU VỰC CÁC MIDDLEWARE ---
                app.UseStaticFiles();
                app.UseCors("AllowVueApp");
                app.UseAuthentication();
                app.UseAuthorization();

                app.MapHub<POS36.Api.Hubs.KitchenHub>("/kitchenHub");
                app.MapControllers();

                Log.Information("✅ Khởi động hoàn tất! POS36 đang lắng nghe tại http://localhost:5198");
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "❌ Server gặp sự cố và phải dừng lại!");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}