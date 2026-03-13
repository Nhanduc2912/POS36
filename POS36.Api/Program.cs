using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

namespace POS36.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Thêm kết nối Database
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 2. Cấu hình JWT
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

            // 3. (SỬA Ở ĐÂY) Cấu hình Swagger thay vì OpenApi của .NET 9
            builder.Services.AddEndpointsApiExplorer();
            // Thêm thư viện này ở đầu file Program.cs
            //

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "POS36 API", Version = "v1" });

                // 1. Cấu hình tạo nút "Authorize" (Ổ khóa) trên Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header sử dụng scheme Bearer. \r\n\r\n Nhập 'Bearer' [khoảng trắng] và dán Token của bạn vào.\r\n\r\nVí dụ: 'Bearer eyJhbGci...'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                // 2. Yêu cầu Swagger tự động nhét Token vào Header mỗi khi gọi API
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
            builder.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173") // Cổng mặc định của Vite
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials(); // Rất quan trọng: Bắt buộc phải có dòng này thì SignalR mới chạy được
        });
});
            var app = builder.Build();

            // 4. (SỬA Ở ĐÂY) Kích hoạt giao diện Swagger UI
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(); // Hàm này sẽ sinh ra cái giao diện web cho em test
            }

            // Authentication phải nằm trên Authorization
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors("AllowVueApp");

            // Đăng ký đường dẫn cho ống nước SignalR
            app.MapHub<POS36.Api.Hubs.KitchenHub>("/kitchenHub");

            app.MapControllers();
            app.Run();
        }
    }
}