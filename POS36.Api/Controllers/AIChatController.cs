using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using POS36.Api.Models;
using POS36.Api.Services;
using Serilog;
using System.Text.Json;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class AIChatController : ControllerBase
    {
        private readonly GeminiAIService _gemini;
        private readonly AppDbContext _context;

        // In-memory conversation history (per session)
        private static readonly Dictionary<string, List<GeminiMessage>> _sessions = new();
        private static readonly object _lock = new();

        public AIChatController(GeminiAIService gemini, AppDbContext context)
        {
            _gemini = gemini;
            _context = context;
        }

        // ==========================================
        // 0. DANH SÁCH MODELS
        // ==========================================
        [HttpGet("models")]
        public async Task<IActionResult> GetModels()
        {
            var models = await _gemini.GetModelsAsync();
            return Ok(models);
        }

        // ==========================================
        // 1. CHAT — Gửi prompt, nhận phản hồi AI
        // ==========================================
        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Prompt))
                return BadRequest(new { error = "Prompt không được để trống!" });

            var sessionId = req.SessionId ?? User.Identity?.Name ?? "default";

            List<GeminiMessage>? history;
            lock (_lock) { _sessions.TryGetValue(sessionId, out history); }

            // Route sang PureChat nếu mode == "chat"
            GeminiResponse response;
            if (req.Mode == "chat")
                response = await _gemini.PureChatAsync(req.Prompt, history, req.ModelId);
            else
                response = await _gemini.ChatAsync(req.Prompt, history, req.ModelId);

            if (response.Error != null)
                return StatusCode(500, new { error = response.Error });

            // Cập nhật history (giới hạn 20 turns)
            lock (_lock)
            {
                if (!_sessions.ContainsKey(sessionId))
                    _sessions[sessionId] = new List<GeminiMessage>();

                _sessions[sessionId].Add(new GeminiMessage { Role = "user", Text = req.Prompt });
                _sessions[sessionId].Add(new GeminiMessage
                {
                    Role = "model",
                    Text = response.Type == GeminiResponseType.Text
                        ? response.Text
                        : $"[FunctionCall: {response.FunctionName}]"
                });
                if (_sessions[sessionId].Count > 40)
                    _sessions[sessionId].RemoveRange(0, 2);
            }

            // Ghi nhật ký
            _context.NhatKyHeThangs.Add(new NhatKyHeThong
            {
                HanhDong = "AIChat",
                MoTa = $"Prompt: \"{TruncateStr(req.Prompt, 100)}\" → {response.Type}",
                NguoiThucHien = User.Identity?.Name,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                ThoiGian = DateTime.Now
            });
            await _context.SaveChangesAsync();

            return Ok(new
            {
                type = response.Type.ToString(),
                text = response.Text,
                requiresAction = response.Type == GeminiResponseType.RequiresAction,
                functionName = response.FunctionName,
                functionArgs = response.FunctionArgs,
                riskLevel = response.RiskLevel,
                displayMessage = response.DisplayMessage,
                sessionId,
                // Token usage & stats
                usage = new {
                    promptTokens = response.Usage.PromptTokens,
                    responseTokens = response.Usage.ResponseTokens,
                    totalTokens = response.Usage.TotalTokens,
                    elapsedMs = response.Usage.ElapsedMs,
                    model = response.Usage.Model
                }
            });
        }

        // ==========================================
        // 2. CONFIRM — Thực thi hoặc hủy lệnh AI
        // ==========================================
        [HttpPost("confirm")]
        public async Task<IActionResult> Confirm([FromBody] ConfirmRequest req)
        {
            if (!req.Confirmed)
            {
                _context.NhatKyHeThangs.Add(new NhatKyHeThong
                {
                    HanhDong = "AIHuyLenh",
                    MoTa = $"Hủy lệnh AI: {req.FunctionName}",
                    NguoiThucHien = User.Identity?.Name,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    ThoiGian = DateTime.Now
                });
                await _context.SaveChangesAsync();
                return Ok(new { success = false, message = "[Aborted] Lệnh đã bị hủy bỏ an toàn theo yêu cầu của Super-Admin." });
            }

            var result = await ExecuteToolAsync(req.FunctionName, req.FunctionArgs ?? "{}");

            _context.NhatKyHeThangs.Add(new NhatKyHeThong
            {
                HanhDong = "AIThucThi",
                MoTa = $"Thực thi: {req.FunctionName} → {(result.Success ? "OK" : "FAIL")}",
                NguoiThucHien = User.Identity?.Name,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                ThoiGian = DateTime.Now,
                ChiTietJson = JsonSerializer.Serialize(new { function = req.FunctionName, args = req.FunctionArgs, result = result.Message })
            });
            await _context.SaveChangesAsync();

            return Ok(new { success = result.Success, message = result.Message, data = result.Data });
        }

        // ==========================================
        // 3. XÓA SESSION
        // ==========================================
        [HttpDelete("session/{sessionId}")]
        public IActionResult ClearSession(string sessionId)
        {
            lock (_lock) { _sessions.Remove(sessionId); }
            return Ok(new { message = "Session đã được xóa." });
        }

        // ==========================================
        // 4. DANH SÁCH TOOLS
        // ==========================================
        [HttpGet("tools")]
        public IActionResult GetTools() =>
            Ok(GeminiAIService.AvailableTools.Select(t => new { t.Name, t.Description, t.RiskLevel }));

        // ==========================================
        // TOOL EXECUTION ENGINE
        // ==========================================
        private async Task<ToolResult> ExecuteToolAsync(string fn, string argsJson)
        {
            try
            {
                var args = JsonDocument.Parse(argsJson).RootElement;
                return fn switch
                {
                    "ThongKeSaaS" => await ToolThongKe(),
                    "DanhSachCuaHang" => await ToolDanhSach(
                        args.TryGetProperty("trangThai", out var tt) ? tt.GetString() : null,
                        args.TryGetProperty("sapHetHan", out var s) && s.GetBoolean()),
                    "KhoaCuaHang" => await ToolKhoa(
                        args.GetProperty("cuaHangId").GetInt32(),
                        args.TryGetProperty("lyDo", out var ld) ? ld.GetString() ?? "AI Agent" : "AI Agent"),
                    "MoKhoaCuaHang" => await ToolMoKhoa(args.GetProperty("cuaHangId").GetInt32()),
                    "GiaHanGoi" => await ToolGiaHan(
                        args.GetProperty("cuaHangId").GetInt32(),
                        args.TryGetProperty("soThang", out var st) ? st.GetInt32() : 1,
                        args.TryGetProperty("goiMoi", out var gm) ? gm.GetString() : null),
                    "GuiThongBao" => await ToolThongBao(
                        args.TryGetProperty("cuaHangId", out var cid) ? cid.GetInt32() : 0,
                        args.GetProperty("tieuDe").GetString() ?? "",
                        args.GetProperty("noiDung").GetString() ?? "",
                        args.TryGetProperty("loai", out var lo) ? lo.GetString() ?? "ThongTin" : "ThongTin"),
                    "XemNhatKy" => await ToolNhatKy(
                        args.TryGetProperty("tuNgay", out var tn) ? tn.GetString() : null,
                        args.TryGetProperty("hanhDong", out var hd) ? hd.GetString() : null),
                    _ => new ToolResult { Success = false, Message = $"Tool '{fn}' không được hỗ trợ." }
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Tool execution error: {Fn}", fn);
                return new ToolResult { Success = false, Message = $"Lỗi thực thi: {ex.Message}" };
            }
        }

        private async Task<ToolResult> ToolThongKe()
        {
            var now = DateTime.Now;
            var data = new
            {
                tongCuaHang = await _context.CuaHangs.CountAsync(),
                hoatDong = await _context.CuaHangs.CountAsync(c => c.TrangThai == "HoatDong"),
                dungThu = await _context.CuaHangs.CountAsync(c => c.TrangThai == "DungThu"),
                chiDoc = await _context.CuaHangs.CountAsync(c => c.TrangThai == "ChiDoc"),
                biKhoa = await _context.CuaHangs.CountAsync(c => c.TrangThai == "BiKhoa"),
                sapHetHan7Ngay = await _context.CuaHangs.CountAsync(c => c.TrangThai == "HoatDong" && c.NgayHetHan <= now.AddDays(7)),
                doanhThuThang = await _context.LichSuDangKys
                    .Where(l => l.TrangThai == "DaThanhToan" && l.NgayThanhToan >= new DateTime(now.Year, now.Month, 1))
                    .SumAsync(l => (decimal?)l.SoTienThanhToan) ?? 0
            };
            return new ToolResult { Success = true, Message = "✅ Thống kê SaaS hiện tại:", Data = data };
        }

        private async Task<ToolResult> ToolDanhSach(string? trangThai, bool sapHetHan)
        {
            var q = _context.CuaHangs.AsQueryable();
            if (!string.IsNullOrEmpty(trangThai)) q = q.Where(c => c.TrangThai == trangThai);
            if (sapHetHan) q = q.Where(c => c.NgayHetHan <= DateTime.Now.AddDays(7));
            var list = await q.Select(c => new { c.Id, c.TenCuaHang, c.TrangThai, c.NgayHetHan, c.GoiDichVu }).Take(20).ToListAsync();
            return new ToolResult { Success = true, Message = $"✅ {list.Count} cửa hàng.", Data = list };
        }

        private async Task<ToolResult> ToolKhoa(int id, string lyDo)
        {
            var ch = await _context.CuaHangs.FindAsync(id);
            if (ch == null) return new ToolResult { Success = false, Message = $"Không tìm thấy cửa hàng #{id}" };
            ch.TrangThai = "BiKhoa"; ch.GhiChu = $"[AI Lock] {lyDo}";
            await _context.SaveChangesAsync();
            return new ToolResult { Success = true, Message = $"✅ Đã khóa: [{ch.TenCuaHang}]" };
        }

        private async Task<ToolResult> ToolMoKhoa(int id)
        {
            var ch = await _context.CuaHangs.FindAsync(id);
            if (ch == null) return new ToolResult { Success = false, Message = $"Không tìm thấy cửa hàng #{id}" };
            ch.TrangThai = ch.NgayHetHan > DateTime.Now ? "HoatDong" : "ChiDoc";
            await _context.SaveChangesAsync();
            return new ToolResult { Success = true, Message = $"✅ Đã mở khóa: [{ch.TenCuaHang}] → {ch.TrangThai}" };
        }

        private async Task<ToolResult> ToolGiaHan(int id, int soThang, string? goiMoi)
        {
            var ch = await _context.CuaHangs.FindAsync(id);
            if (ch == null) return new ToolResult { Success = false, Message = $"Không tìm thấy cửa hàng #{id}" };
            ch.NgayHetHan = (ch.NgayHetHan > DateTime.Now ? ch.NgayHetHan : DateTime.Now).AddMonths(soThang);
            ch.TrangThai = "HoatDong";
            if (!string.IsNullOrEmpty(goiMoi)) ch.GoiDichVu = goiMoi;
            await _context.SaveChangesAsync();
            return new ToolResult { Success = true, Message = $"✅ Đã gia hạn [{ch.TenCuaHang}] +{soThang} tháng → {ch.NgayHetHan:dd/MM/yyyy}" };
        }

        private async Task<ToolResult> ToolThongBao(int cuaHangId, string tieuDe, string noiDung, string loai)
        {
            _context.ThongBaoHeThongs.Add(new ThongBaoHeThong
            {
                TieuDe = tieuDe, NoiDung = noiDung, LoaiThongBao = loai,
                CuaHangId = cuaHangId == 0 ? null : cuaHangId,
                NgayTao = DateTime.Now, DaDoc = false
            });
            await _context.SaveChangesAsync();
            return new ToolResult { Success = true, Message = $"✅ Đã gửi thông báo đến {(cuaHangId == 0 ? "tất cả" : $"quán #{cuaHangId}")}" };
        }

        private async Task<ToolResult> ToolNhatKy(string? tuNgay, string? hanhDong)
        {
            var q = _context.NhatKyHeThangs.AsQueryable();
            if (DateTime.TryParse(tuNgay, out var from)) q = q.Where(n => n.ThoiGian >= from);
            if (!string.IsNullOrEmpty(hanhDong)) q = q.Where(n => n.HanhDong == hanhDong);
            var logs = await q.OrderByDescending(n => n.ThoiGian).Take(20)
                .Select(n => new { n.HanhDong, n.MoTa, n.NguoiThucHien, n.ThoiGian }).ToListAsync();
            return new ToolResult { Success = true, Message = $"✅ {logs.Count} bản ghi.", Data = logs };
        }

        private static string TruncateStr(string s, int max) => s.Length <= max ? s : s[..max] + "...";
    }

    public class ChatRequest
    {
        public string Prompt { get; set; } = "";
        public string? SessionId { get; set; }
        public string? ModelId { get; set; }       // gemini-1.5-flash, gemini-1.5-pro, etc.
        public string Mode { get; set; } = "agent"; // "agent" | "chat"
    }
    public class ConfirmRequest { public bool Confirmed { get; set; } public string FunctionName { get; set; } = ""; public string? FunctionArgs { get; set; } }
    public class ToolResult { public bool Success { get; set; } public string Message { get; set; } = ""; public object? Data { get; set; } }
}