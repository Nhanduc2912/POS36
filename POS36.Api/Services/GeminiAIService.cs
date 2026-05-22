using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Serilog;

namespace POS36.Api.Services
{
    /// <summary>
    /// Service giao tiếp với Gemini API.
    /// Hỗ trợ: Function Calling, multi-turn, model selector, token usage.
    /// </summary>
    public class GeminiAIService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private const string BASE = "https://generativelanguage.googleapis.com/v1beta";
        private const string DEFAULT_MODEL = "gemini-3.1-flash-lite";

        public static readonly List<GeminiTool> AvailableTools = new()
        {
            new GeminiTool("ThongKeSaaS",      "Xem thống kê tổng quan SaaS: số quán, doanh thu, sắp hết hạn"),
            new GeminiTool("DanhSachCuaHang",  "Danh sách cửa hàng. Params: trangThai, sapHetHan (bool)"),
            new GeminiTool("KhoaCuaHang",      "Khóa cửa hàng. Params: cuaHangId, lyDo")                    { RiskLevel = "HIGH" },
            new GeminiTool("MoKhoaCuaHang",    "Mở khóa cửa hàng. Params: cuaHangId")                       { RiskLevel = "MEDIUM" },
            new GeminiTool("GiaHanGoi",        "Gia hạn gói. Params: cuaHangId, soThang, goiMoi")            { RiskLevel = "LOW" },
            new GeminiTool("GuiThongBao",      "Gửi thông báo. Params: cuaHangId (0=all), tieuDe, noiDung, loai"),
            new GeminiTool("XemNhatKy",        "Xem nhật ký. Params: tuNgay (yyyy-MM-dd), hanhDong"),
            new GeminiTool("ThemGoiSaaS",      "Thêm gói dịch vụ SaaS. Params: tenGoi, maGoi, soThang, giaThang, tongGia, gioiHanHoaDon, gioiHanNhanVien, moTa") { RiskLevel = "HIGH" },
            new GeminiTool("XuatBaoCaoAI",     "Sinh ra báo cáo thống kê chuyên sâu dạng mã HTML. Dùng khi người dùng yêu cầu lập báo cáo, vẽ bảng thống kê. Params: htmlContent") { RiskLevel = "LOW" },
            new GeminiTool("ThietLapHeThong",  "Chỉnh sửa cấu hình toàn hệ thống (SuperAdmin). Params: key, value") { RiskLevel = "HIGH" },
        };

        public GeminiAIService(IConfiguration config, HttpClient http)
        {
            _apiKey = config["GeminiAI:ApiKey"]
                ?? throw new InvalidOperationException("GeminiAI:ApiKey chưa cấu hình trong User Secrets!");
            _http = http;
        }

        // =============================================
        // 1. LẤY DANH SÁCH MODELS TỪ GOOGLE AI STUDIO
        // =============================================
        public async Task<List<GeminiModelInfo>> GetModelsAsync()
        {
            try
            {
                var url = $"{BASE}/models?key={_apiKey}&pageSize=50";
                var res = await _http.GetAsync(url);
                var json = await res.Content.ReadAsStringAsync();

                if (!res.IsSuccessStatusCode)
                {
                    Log.Warning("GetModels failed: {Status} {Body}", res.StatusCode, json[..Math.Min(json.Length, 200)]);
                    return GetFallbackModels();
                }

                using var doc = JsonDocument.Parse(json);
                var models = new List<GeminiModelInfo>();

                if (!doc.RootElement.TryGetProperty("models", out var modelsEl))
                    return GetFallbackModels();

                foreach (var m in modelsEl.EnumerateArray())
                {
                    var name = m.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                    var displayName = m.TryGetProperty("displayName", out var dn) ? dn.GetString() ?? "" : "";
                    var description = m.TryGetProperty("description", out var d) ? d.GetString() ?? "" : "";
                    var inputLimit = m.TryGetProperty("inputTokenLimit", out var il) ? il.GetInt64() : 0;
                    var outputLimit = m.TryGetProperty("outputTokenLimit", out var ol) ? ol.GetInt64() : 0;

                    // Chỉ lấy model hỗ trợ generateContent
                    bool supportsGenerate = false;
                    if (m.TryGetProperty("supportedGenerationMethods", out var methods))
                        supportsGenerate = methods.EnumerateArray().Any(x => x.GetString() == "generateContent");

                    if (!supportsGenerate) continue;

                    // Bỏ qua embedding và legacy models
                    if (name.Contains("embedding") || name.Contains("aqa")) continue;

                    models.Add(new GeminiModelInfo
                    {
                        Id = name.Replace("models/", ""),
                        FullName = name,
                        DisplayName = string.IsNullOrEmpty(displayName) ? name.Replace("models/", "") : displayName,
                        Description = description[..Math.Min(description.Length, 120)],
                        InputTokenLimit = inputLimit,
                        OutputTokenLimit = outputLimit,
                        IsDefault = name.Contains("gemini-3.1-flash-lite")
                    });
                }

                return models.Count > 0 ? models : GetFallbackModels();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "GetModelsAsync exception");
                return GetFallbackModels();
            }
        }

        private static List<GeminiModelInfo> GetFallbackModels() => new()
        {
            new() { Id = "gemini-3.1-flash-lite",DisplayName = "Gemini 3.1 Flash Lite",Description = "Nhanh, siêu nhẹ, tiết kiệm",          InputTokenLimit = 1000000, IsDefault = true },
            new() { Id = "gemini-3.5-flash",     DisplayName = "Gemini 3.5 Flash",    Description = "Cân bằng hiệu năng tốt nhất",            InputTokenLimit = 1000000 },
            new() { Id = "gemini-2.5-flash",     DisplayName = "Gemini 2.5 Flash",    Description = "Thế hệ 2.5 nhanh & mạnh",                InputTokenLimit = 1000000 },
            new() { Id = "gemini-3.1-pro-preview",DisplayName = "Gemini 3.1 Pro Preview",Description = "Thông minh nhất thế hệ mới",         InputTokenLimit = 2000000 },
        };

        // =============================================
        // 2. CHAT (Agent mode — Function Calling)
        // =============================================
        public async Task<GeminiResponse> ChatAsync(string prompt, List<GeminiMessage>? history = null, string? modelId = null)
        {
            var model = modelId ?? DEFAULT_MODEL;
            var url = $"{BASE}/models/{model}:generateContent?key={_apiKey}";

            var systemInstruction = LoadPrompt("SuperAdmin_Agent.md");

            var contents = new List<object>();
            if (history != null)
                foreach (var msg in history)
                    contents.Add(new { role = msg.Role, parts = new[] { new { text = msg.Text } } });
            contents.Add(new { role = "user", parts = new[] { new { text = prompt } } });

            var toolDeclarations = AvailableTools.Select(t => new { name = t.Name, description = t.Description }).ToList();

            var body = new
            {
                system_instruction = new { parts = new[] { new { text = systemInstruction } } },
                contents,
                tools = new[] { new { function_declarations = toolDeclarations } },
                tool_config = new { function_calling_config = new { mode = "AUTO" } },
                generation_config = new { temperature = 0.4, max_output_tokens = 2048 }
            };

            return await SendRequestAsync(url, body, model);
        }

        // =============================================
        // 3. AI REPORT — Gemini tự tạo HTML từ dữ liệu thực
        // =============================================
        public async Task<string> GenerateReportWithAI(string userPrompt, object realData, string? modelId = null)
        {
            var model = modelId ?? DEFAULT_MODEL; // Tránh lỗi limit: 0 của gemini-2.0-flash-lite trên free tier
            var url = $"{BASE}/models/{model}:generateContent?key={_apiKey}";

            var systemPrompt = LoadPrompt("SuperAdmin_Agent.md");

            // Serialize data đẹp để AI đọc
            var dataJson = JsonSerializer.Serialize(realData, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            var reportPrompt = $"""
YÊU CẦU BÁO CÁO: {userPrompt}

DỮ LIỆU THỰC TỪ DATABASE (JSON):
{dataJson}

Hãy tạo báo cáo HTML hoàn chỉnh dựa trên dữ liệu trên.
QUAN TRỌNG:
- Chỉ trả về HTML thuần túy, không có markdown, không có ```html
- Dùng dark theme: background #0f1117, accent #f59e0b
- Format linh hoạt theo yêu cầu (bảng, stats, phân tích...)
- Thêm nhận xét/insight ngắn từ dữ liệu thực
""";

            var body = new
            {
                system_instruction = new { parts = new[] { new { text = systemPrompt } } },
                contents = new[] { new { role = "user", parts = new[] { new { text = reportPrompt } } } },
                generation_config = new { temperature = 0.6, max_output_tokens = 8192 }
            };

            var response = await SendRequestAsync(url, body, model);
            if (response.Error != null)
                return $"<div style='color:#ef4444;padding:16px'>⚠️ Lỗi AI: {response.Error}</div>";

            var html = response.Text?.Trim() ?? "";
            // Clean markdown nếu AI vẫn trả về
            if (html.StartsWith("```html")) html = html[7..];
            if (html.StartsWith("```")) html = html[3..];
            if (html.EndsWith("```")) html = html[..^3];
            return html.Trim();
        }

        // Load prompt file từ Prompts/
        private static string? _cachedSuperAdminPrompt;
        private static string LoadPrompt(string fileName)
        {
            if (_cachedSuperAdminPrompt != null) return _cachedSuperAdminPrompt;
            try
            {
                // Tìm file từ thư mục ứng dụng
                var basePaths = new[]
                {
                    Path.Combine(AppContext.BaseDirectory, "Prompts", fileName),
                    Path.Combine(Directory.GetCurrentDirectory(), "Prompts", fileName),
                    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Prompts", fileName),
                };
                foreach (var p in basePaths)
                {
                    if (File.Exists(p))
                    {
                        _cachedSuperAdminPrompt = File.ReadAllText(p);
                        Log.Information("Loaded AI prompt: {Path}", p);
                        return _cachedSuperAdminPrompt;
                    }
                }
                Log.Warning("Prompt file not found: {File}", fileName);
            }
            catch (Exception ex) { Log.Error(ex, "LoadPrompt error"); }

            // Fallback ngắn gọn
            return "Bạn là AI Agent SuperAdmin của POS36 SaaS. Trả lời tiếng Việt. Khi tạo báo cáo chỉ dùng HTML thuần, dark theme nền #0f1117.";
        }




        // =============================================
        // 3. PURE CHAT (Chỉ hội thoại, không function calling)
        // =============================================
        public async Task<GeminiResponse> PureChatAsync(string prompt, List<GeminiMessage>? history = null, string? modelId = null)
        {
            var model = modelId ?? DEFAULT_MODEL;
            var url = $"{BASE}/models/{model}:generateContent?key={_apiKey}";

            var contents = new List<object>();
            if (history != null)
                foreach (var msg in history)
                    contents.Add(new { role = msg.Role, parts = new[] { new { text = msg.Text } } });
            contents.Add(new { role = "user", parts = new[] { new { text = prompt } } });

            var body = new
            {
                contents,
                generation_config = new { temperature = 0.7, max_output_tokens = 4096 }
            };

            return await SendRequestAsync(url, body, model);
        }

        // =============================================
        // INTERNAL: Gửi request và parse response
        // =============================================
        private async Task<GeminiResponse> SendRequestAsync(string url, object body, string model)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var json = JsonSerializer.Serialize(body);
                var req = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                var res = await _http.SendAsync(req);
                var content = await res.Content.ReadAsStringAsync();
                sw.Stop();

                if (!res.IsSuccessStatusCode)
                {
                    Log.Error("Gemini API {Status}: {Body}", res.StatusCode, content[..Math.Min(content.Length, 300)]);
                    return new GeminiResponse { Error = $"API lỗi {res.StatusCode}: {ExtractError(content)}" };
                }

                return ParseResponse(content, model, sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();
                Log.Error(ex, "Gemini request exception");
                return new GeminiResponse { Error = ex.Message };
            }
        }

        private GeminiResponse ParseResponse(string raw, string model, long elapsedMs)
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            // Token usage
            var usage = new GeminiUsage();
            if (root.TryGetProperty("usageMetadata", out var um))
            {
                usage.PromptTokens = um.TryGetProperty("promptTokenCount", out var pt) ? pt.GetInt32() : 0;
                usage.ResponseTokens = um.TryGetProperty("candidatesTokenCount", out var ct) ? ct.GetInt32() : 0;
                usage.TotalTokens = um.TryGetProperty("totalTokenCount", out var tt) ? tt.GetInt32() : 0;
            }
            usage.ElapsedMs = elapsedMs;
            usage.Model = model;

            if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
                return new GeminiResponse { Error = "Không có candidates trong response", Usage = usage };

            var candidate = candidates[0].GetProperty("content").GetProperty("parts")[0];

            // Function Call?
            if (candidate.TryGetProperty("functionCall", out var funcCall))
            {
                var funcName = funcCall.GetProperty("name").GetString() ?? "";
                var args = funcCall.TryGetProperty("args", out var a) ? a.GetRawText() : "{}";
                var tool = AvailableTools.FirstOrDefault(t => t.Name == funcName);
                return new GeminiResponse
                {
                    Type = GeminiResponseType.RequiresAction,
                    FunctionName = funcName,
                    FunctionArgs = args,
                    RiskLevel = tool?.RiskLevel ?? "LOW",
                    DisplayMessage = $"AI muốn thực thi: **{funcName}**",
                    Usage = usage
                };
            }

            // Text response
            if (candidate.TryGetProperty("text", out var textEl))
                return new GeminiResponse { Type = GeminiResponseType.Text, Text = textEl.GetString() ?? "", Usage = usage };

            return new GeminiResponse { Error = "Không thể parse response", Usage = usage };
        }

        private static string ExtractError(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("error", out var err) &&
                    err.TryGetProperty("message", out var msg))
                    return msg.GetString() ?? json;
            }
            catch { }
            return json[..Math.Min(json.Length, 100)];
        }
    }

    // ===== DTOs =====
    public enum GeminiResponseType { Text, RequiresAction, Error }

    public class GeminiResponse
    {
        public GeminiResponseType Type { get; set; } = GeminiResponseType.Text;
        public string Text { get; set; } = "";
        public string? FunctionName { get; set; }
        public string? FunctionArgs { get; set; }
        public string RiskLevel { get; set; } = "LOW";
        public string? DisplayMessage { get; set; }
        public string? Error { get; set; }
        public GeminiUsage Usage { get; set; } = new();
    }

    public class GeminiUsage
    {
        public int PromptTokens { get; set; }
        public int ResponseTokens { get; set; }
        public int TotalTokens { get; set; }
        public long ElapsedMs { get; set; }
        public string Model { get; set; } = "";
    }

    public class GeminiMessage { public string Role { get; set; } = "user"; public string Text { get; set; } = ""; }
    public class GeminiTool { public string Name { get; set; } public string Description { get; set; } public string RiskLevel { get; set; } = "LOW"; public GeminiTool(string n, string d) { Name = n; Description = d; } }
    public class GeminiModelInfo
    {
        public string Id { get; set; } = "";
        public string FullName { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Description { get; set; } = "";
        public long InputTokenLimit { get; set; }
        public long OutputTokenLimit { get; set; }
        public bool IsDefault { get; set; }
    }
}
