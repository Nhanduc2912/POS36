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

            // 1. Ưu tiên đọc cấu hình từ appsettings.json (Bảo mật tối đa, không sợ lộ ra UI)
            string provider = _configuration["CloudStorage:Provider"] ?? "Cloudinary";
            string cloudinaryCloudName = _configuration["CloudStorage:CloudName"] ?? "kab6azhv";
            string cloudinaryPreset = _configuration["CloudStorage:UploadPreset"] ?? "pos36_preset";
            string imgBbKey = _configuration["CloudStorage:ImgBbApiKey"] ?? "";

            // 2. Nếu cấu hình appsettings trống, mới đọc từ DB
            if (string.IsNullOrEmpty(cloudinaryCloudName) || string.IsNullOrEmpty(cloudinaryPreset))
            {
                var cloudKeys = new[] { "CloudProvider", "CloudinaryCloudName", "CloudinaryUploadPreset", "ImgBbApiKey" };
                var configs = await _context.CauHinhHeThangs
                    .Where(c => cloudKeys.Contains(c.MaKey))
                    .ToDictionaryAsync(c => c.MaKey, c => c.GiaTri);

                provider = configs.GetValueOrDefault("CloudProvider", provider);
                cloudinaryCloudName = configs.GetValueOrDefault("CloudinaryCloudName", cloudinaryCloudName);
                cloudinaryPreset = configs.GetValueOrDefault("CloudinaryUploadPreset", cloudinaryPreset);
                imgBbKey = configs.GetValueOrDefault("ImgBbApiKey", imgBbKey);
            }

            // Fallback cứng nếu thiếu
            if (string.IsNullOrEmpty(cloudinaryCloudName)) cloudinaryCloudName = "kab6azhv";
            if (string.IsNullOrEmpty(cloudinaryPreset)) cloudinaryPreset = "pos36_preset";

            // ===== MÔ HÌNH 1: UPLOAD LÊN CLOUDINARY =====
            if (provider.Equals("Cloudinary", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrEmpty(cloudinaryCloudName) &&
                !string.IsNullOrEmpty(cloudinaryPreset))
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
                    Log.Error("❌ Lỗi upload Cloudinary: {Message}. Chuyển sang fallback...", ex.Message);
                }
            }

            // ===== MÔ HÌNH 2: UPLOAD LÊN IMGBB =====
            if (provider.Equals("ImgBB", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(imgBbKey))
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
            return await SaveLocalAsync(file);
        }

        private async Task<string> UploadToCloudinaryAsync(IFormFile file, string cloudName, string uploadPreset, string folder)
        {
            var client = _httpClientFactory.CreateClient();
            var apiUrl = $"https://api.cloudinary.com/v1_1/{cloudName}/image/upload";

            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "image/jpeg");

            content.Add(streamContent, "file", file.FileName);
            content.Add(new StringContent(uploadPreset), "upload_preset");
            content.Add(new StringContent(folder), "folder");

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
            var apiUrl = $"https://api.imgbb.com/1/upload?key={apiKey}";

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
