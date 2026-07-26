using System.Net.Http.Headers;
using System.Text.Json;

namespace POS36.Api.Services
{
    public interface ICloudStorageService
    {
        Task<string?> UploadImageAsync(IFormFile file, string folder = "pos36");
    }

    public class CloudStorageService : ICloudStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<CloudStorageService> _logger;

        public CloudStorageService(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment env,
            ILogger<CloudStorageService> logger)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _env = env;
            _logger = logger;
        }

        public async Task<string?> UploadImageAsync(IFormFile file, string folder = "pos36")
        {
            if (file == null || file.Length == 0) return null;

            // 1. Thử Upload lên Cloudinary nếu có cấu hình CloudName & Preset / ApiKey
            var cloudName = _configuration["Cloudinary:CloudName"];
            var uploadPreset = _configuration["Cloudinary:UploadPreset"];

            if (!string.IsNullOrEmpty(cloudName) && !string.IsNullOrEmpty(uploadPreset))
            {
                try
                {
                    var cloudinaryUrl = await UploadToCloudinaryAsync(file, cloudName, uploadPreset, folder);
                    if (!string.IsNullOrEmpty(cloudinaryUrl))
                    {
                        _logger.LogInformation("Upload thành công lên Cloudinary CDN: {Url}", cloudinaryUrl);
                        return cloudinaryUrl;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Cloudinary Upload thất bại, fallback sang ImgBB / Local: {Error}", ex.Message);
                }
            }

            // 2. Thử Upload lên ImgBB nếu có ImgBBApiKey
            var imgbbApiKey = _configuration["ImgBB:ApiKey"];
            if (!string.IsNullOrEmpty(imgbbApiKey))
            {
                try
                {
                    var imgbbUrl = await UploadToImgBBAsync(file, imgbbApiKey);
                    if (!string.IsNullOrEmpty(imgbbUrl))
                    {
                        _logger.LogInformation("Upload thành công lên ImgBB CDN: {Url}", imgbbUrl);
                        return imgbbUrl;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("ImgBB Upload thất bại, fallback sang Local storage: {Error}", ex.Message);
                }
            }

            // 3. Fallback: Lưu vào wwwroot/images nếu chưa cấu hình Cloud hoặc Cloud bị lỗi
            return await SaveLocalImageAsync(file);
        }

        private async Task<string?> UploadToCloudinaryAsync(IFormFile file, string cloudName, string uploadPreset, string folder)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"https://api.cloudinary.com/v1_1/{cloudName}/image/upload";

            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "image/jpeg");

            content.Add(streamContent, "file", file.FileName);
            content.Add(new StringContent(uploadPreset), "upload_preset");
            if (!string.IsNullOrEmpty(folder))
            {
                content.Add(new StringContent(folder), "folder");
            }

            var response = await client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("secure_url", out var secureUrlProp))
                {
                    return secureUrlProp.GetString();
                }
            }

            return null;
        }

        private async Task<string?> UploadToImgBBAsync(IFormFile file, string apiKey)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"https://api.imgbb.com/1/upload?key={apiKey}";

            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "image/jpeg");

            content.Add(streamContent, "image", file.FileName);

            var response = await client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.GetProperty("data").TryGetProperty("url", out var urlProp))
                {
                    return urlProp.GetString();
                }
            }

            return null;
        }

        private async Task<string> SaveLocalImageAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "images");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/images/{uniqueFileName}";
        }
    }
}
