using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using Serilog;

namespace POS36.Api.Services
{
    public interface ICloudStorageService
    {
        Task<string?> UploadImageAsync(IFormFile? file, string folder = "pos36");
    }

    public class CloudStorageService : ICloudStorageService
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public CloudStorageService(
            AppDbContext context,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public async Task<string?> UploadImageAsync(IFormFile? file, string folder = "pos36")
        {
            if (file == null || file.Length == 0) return null;

            // 1. Lấy thông số từ appsettings.json
            string provider = _configuration["CloudStorage:Provider"] ?? "Cloudinary";
            string cloudinaryCloudName = _configuration["CloudStorage:CloudName"] ?? "kab6azhv";
            string cloudinaryPreset = _configuration["CloudStorage:UploadPreset"] ?? "pos36_preset";
            string imgBbKey = _configuration["CloudStorage:ImgBbApiKey"] ?? "";

            // 2. Kiểm tra nếu DB có cấu hình khác hợp lệ (không rỗng) thì dùng
            try
            {
                var cloudKeys = new[] { "CloudProvider", "CloudinaryCloudName", "CloudinaryUploadPreset", "ImgBbApiKey" };
                var configs = await _context.CauHinhHeThangs
                    .Where(c => cloudKeys.Contains(c.MaKey))
                    .ToDictionaryAsync(c => c.MaKey, c => c.GiaTri);

                var dbProvider = configs.GetValueOrDefault("CloudProvider")?.Trim();
                var dbCloudName = configs.GetValueOrDefault("CloudinaryCloudName")?.Trim();
                var dbPreset = configs.GetValueOrDefault("CloudinaryUploadPreset")?.Trim();
                var dbImgBb = configs.GetValueOrDefault("ImgBbApiKey")?.Trim();

                if (!string.IsNullOrEmpty(dbProvider)) provider = dbProvider;
                if (!string.IsNullOrEmpty(dbCloudName)) cloudinaryCloudName = dbCloudName;
                if (!string.IsNullOrEmpty(dbPreset)) cloudinaryPreset = dbPreset;
                if (!string.IsNullOrEmpty(dbImgBb)) imgBbKey = dbImgBb;
            }
            catch (Exception ex)
            {
                Log.Warning("⚠️ Không thể đọc cấu hình Cloud từ DB: {Message}", ex.Message);
            }

            // Fallback cứng nếu rỗng
            if (string.IsNullOrWhiteSpace(cloudinaryCloudName)) cloudinaryCloudName = "kab6azhv";
            if (string.IsNullOrWhiteSpace(cloudinaryPreset)) cloudinaryPreset = "pos36_preset";

            // ===== MÔ HÌNH 1: UPLOAD LÊN CLOUDINARY =====
            if (provider.Equals("Cloudinary", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    string cloudUrl = await UploadToCloudinaryAsync(file, cloudinaryCloudName, cloudinaryPreset, folder);
                    if (!string.IsNullOrEmpty(cloudUrl))
                    {
                        Log.Information("☁️ Up ảnh thành công lên Cloudinary ({CloudName}): {Url}", cloudinaryCloudName, cloudUrl);
                        return cloudUrl;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("❌ Lỗi upload Cloudinary ({CloudName} / Preset: {Preset}): {Message}. Chuyển sang fallback...", 
                        cloudinaryCloudName, cloudinaryPreset, ex.Message);
                }
            }

            // ===== MÔ HÌNH 2: UPLOAD LÊN IMGBB =====
            if (provider.Equals("ImgBB", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(imgBbKey))
            {
                try
                {
                    string cloudUrl = await UploadToImgBbAsync(file, imgBbKey);
                    if (!string.IsNullOrEmpty(cloudUrl))
                    {
                        Log.Information("☁️ Up ảnh thành công lên ImgBB: {Url}", cloudUrl);
                        return cloudUrl;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("❌ Lỗi upload ImgBB: {Message}. Chuyển sang fallback...", ex.Message);
                }
            }

            // ===== FALLBACK: LƯU CỤC BỘ VÀO WWWROOT/IMAGES =====
            Log.Warning("⚠️ Đang lưu ảnh vào Local fallback (wwwroot/images)");
            return await SaveLocalAsync(file);
        }

        private async Task<string> UploadToCloudinaryAsync(IFormFile file, string cloudName, string uploadPreset, string folder)
        {
            var client = _httpClientFactory.CreateClient();
            var apiUrl = $"https://api.cloudinary.com/v1_1/{cloudName.Trim()}/image/upload";

            using var content = new MultipartFormDataContent();
            
            // LƯU Ý QUAN TRỌNG: Cloudinary API bắt buộc upload_preset phải đứng ĐẦU TIÊN trước file binary
            content.Add(new StringContent(uploadPreset.Trim()), "upload_preset");
            if (!string.IsNullOrWhiteSpace(folder))
            {
                content.Add(new StringContent(folder.Trim()), "folder");
            }

            using var stream = file.OpenReadStream();
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "image/jpeg");
            content.Add(streamContent, "file", file.FileName);

            var response = await client.PostAsync(apiUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(responseString);
                if (doc.RootElement.TryGetProperty("secure_url", out var urlProp))
                {
                    return urlProp.GetString() ?? "";
                }
            }

            throw new Exception($"Cloudinary API Error ({response.StatusCode}): {responseString}");
        }

        private async Task<string> UploadToImgBbAsync(IFormFile file, string apiKey)
        {
            var client = _httpClientFactory.CreateClient();
            var apiUrl = $"https://api.imgbb.com/1/upload?key={apiKey.Trim()}";

            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            var streamContent = new StreamContent(stream);

            content.Add(streamContent, "image", file.FileName);

            var response = await client.PostAsync(apiUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(responseString);
                if (doc.RootElement.TryGetProperty("data", out var dataObj) &&
                    dataObj.TryGetProperty("url", out var urlProp))
                {
                    return urlProp.GetString() ?? "";
                }
            }

            throw new Exception($"ImgBB API Error ({response.StatusCode}): {responseString}");
        }

        private async Task<string> SaveLocalAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/images/" + uniqueFileName;
        }
    }
}
