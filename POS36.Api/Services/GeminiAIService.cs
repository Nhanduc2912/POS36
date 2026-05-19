using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Serilog;

namespace POS36.Api.Services
{
    /// <summary>
    /// Service giao tiếp với Gemini API (Function Calling).
    /// Hỗ trợ multi-turn conversation và tool execution flow.
    /// </summary>
    public class GeminiAIService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly string _model = "gemini-1.5-flash";

        // Định nghĩa các tools mà AI có thể gọi
        public static readonly List<GeminiTool> AvailableTools = new()
        {
            new GeminiTool("ThongKeSaaS", "Xem thống kê tổng quan hệ thống SaaS: số quán, doanh thu, quán sắp hết hạn"),
            new GeminiTool("DanhSachCuaHang", "Lấy danh sách cửa hàng với bộ lọc. Tham số: trangThai (HoatDong/ChiDoc/BiKhoa/DungThu), sapHetHan (true/false)"),
            new GeminiTool("KhoaCuaHang", "Khóa cửa hàng. Tham số: cuaHangId (int), lyDo (string)") { RiskLevel = "HIGH" },
            new GeminiTool("MoKhoaCuaHang", "Mở khóa cửa hàng. Tham số: cuaHangId (int)") { RiskLevel = "MEDIUM" },
            new GeminiTool("GiaHanGoi", "Gia hạn gói dịch vụ cho cửa hàng. Tham số: cuaHangId (int), soThang (int), goiMoi (STARTER/PRO/ULTIMATE)") { RiskLevel = "LOW" },
            new GeminiTool("GuiThongBao", "Gửi thông báo hệ thống. Tham số: cuaHangId (int hoặc 0=tất cả), tieuDe (string), noiDung (string), loai (ThongTin/CanhBao/KhanCap)"),
            new GeminiTool("XemNhatKy", "Xem nhật ký hệ thống. Tham số: tuNgay (yyyy-MM-dd), hanhDong (tùy chọn)"),
        };

        public GeminiAIService(IConfiguration config, HttpClient http)
        {
            _apiKey = config["GeminiAI:ApiKey"] ?? throw new InvalidOperationException("GeminiAI:ApiKey chưa được cấu hình!");
            _http = http;
        }

        public async Task<GeminiResponse> ChatAsync(string prompt, List<GeminiMessage>? history = null)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

            // Build system instruction
            var systemInstruction = @"Bạn là AI Assistant của hệ thống POS36 SaaS. 
Nhiệm vụ: Hỗ trợ Super Admin quản lý toàn bộ hệ thống cửa hàng.
Quy tắc:
- Luôn phản hồi bằng tiếng Việt
- Với các lệnh NGUY HIỂM (khóa quán, xóa dữ liệu), phải xác nhận rõ ràng trước khi thực thi
- Chỉ thực hiện các tool đã được định nghĩa
- Nếu không chắc, hỏi lại Super Admin";

            // Build conversation history
            var contents = new List<object>();
            if (history != null)
                foreach (var msg in history)
                    contents.Add(new { role = msg.Role, parts = new[] { new { text = msg.Text } } });

            contents.Add(new { role = "user", parts = new[] { new { text = prompt } } });

            // Build tool declarations
            var toolDeclarations = AvailableTools.Select(t => new
            {
                name = t.Name,
                description = t.Description,
            }).ToList();

            var body = new
            {
                system_instruction = new { parts = new[] { new { text = systemInstruction } } },
                contents,
                tools = new[] { new { function_declarations = toolDeclarations } },
                tool_config = new { function_calling_config = new { mode = "AUTO" } },
                generation_config = new { temperature = 0.3, max_output_tokens = 2048 }
            };

            var json = JsonSerializer.Serialize(body);
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            try
            {
                var res = await _http.SendAsync(request);
                var content = await res.Content.ReadAsStringAsync();

                if (!res.IsSuccessStatusCode)
                {
                    Log.Error("Gemini API error {StatusCode}: {Content}", res.StatusCode, content);
                    return new GeminiResponse { Error = $"Gemini API lỗi: {res.StatusCode}" };
                }

                return ParseResponse(content);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Gemini API exception");
                return new GeminiResponse { Error = ex.Message };
            }
        }

        private GeminiResponse ParseResponse(string raw)
        {
            using var doc = JsonDocument.Parse(raw);
            var candidate = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0];

            // Kiểm tra functionCall
            if (candidate.TryGetProperty("functionCall", out var funcCall))
            {
                var funcName = funcCall.GetProperty("name").GetString() ?? "";
                var args = funcCall.TryGetProperty("args", out var argsEl)
                    ? argsEl.GetRawText()
                    : "{}";

                var tool = AvailableTools.FirstOrDefault(t => t.Name == funcName);

                return new GeminiResponse
                {
                    Type = GeminiResponseType.RequiresAction,
                    FunctionName = funcName,
                    FunctionArgs = args,
                    RiskLevel = tool?.RiskLevel ?? "LOW",
                    DisplayMessage = $"AI muốn thực thi: **{funcName}**\nTham số: `{args}`"
                };
            }

            // Trả về text
            if (candidate.TryGetProperty("text", out var textEl))
            {
                return new GeminiResponse
                {
                    Type = GeminiResponseType.Text,
                    Text = textEl.GetString() ?? ""
                };
            }

            return new GeminiResponse { Error = "Không thể phân tích phản hồi từ Gemini" };
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
        public string RiskLevel { get; set; } = "LOW"; // LOW | MEDIUM | HIGH
        public string? DisplayMessage { get; set; }
        public string? Error { get; set; }
    }

    public class GeminiMessage
    {
        public string Role { get; set; } = "user"; // user | model
        public string Text { get; set; } = "";
    }

    public class GeminiTool
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string RiskLevel { get; set; } = "LOW";
        public GeminiTool(string name, string desc) { Name = name; Description = desc; }
    }
}
