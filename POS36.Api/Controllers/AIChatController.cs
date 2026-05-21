using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using POS36.Api.Models;
using POS36.Api.Services;
using Serilog;
using System.Text;
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

        private static readonly Dictionary<string, List<GeminiMessage>> _sessions = new();
        private static readonly object _lock = new();

        public AIChatController(GeminiAIService gemini, AppDbContext context)
        {
            _gemini = gemini;
            _context = context;
        }

        [HttpGet("models")]
        public async Task<IActionResult> GetModels()
        {
            var models = await _gemini.GetModelsAsync();
            return Ok(models);
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Prompt))
                return BadRequest(new { error = "Prompt không được để trống!" });

            var sessionId = req.SessionId ?? User.Identity?.Name ?? "default";
            List<GeminiMessage>? history;
            lock (_lock) { _sessions.TryGetValue(sessionId, out history); }

            GeminiResponse response;
            if (req.Mode == "chat")
                response = await _gemini.PureChatAsync(req.Prompt, history, req.ModelId);
            else
                response = await _gemini.ChatAsync(req.Prompt, history, req.ModelId);

            if (response.Error != null)
                return StatusCode(500, new { error = response.Error });

            lock (_lock)
            {
                if (!_sessions.ContainsKey(sessionId))
                    _sessions[sessionId] = new List<GeminiMessage>();
                _sessions[sessionId].Add(new GeminiMessage { Role = "user", Text = req.Prompt });
                _sessions[sessionId].Add(new GeminiMessage
                {
                    Role = "model",
                    Text = response.Type == GeminiResponseType.Text ? response.Text : $"[FunctionCall: {response.FunctionName}]"
                });
                if (_sessions[sessionId].Count > 40) _sessions[sessionId].RemoveRange(0, 2);
            }

            _context.NhatKyHeThangs.Add(new NhatKyHeThong
            {
                HanhDong = "AIChat",
                MoTa = $"Prompt: \"{TruncateStr(req.Prompt, 100)}\" => {response.Type}",
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
                usage = new
                {
                    promptTokens = response.Usage.PromptTokens,
                    responseTokens = response.Usage.ResponseTokens,
                    totalTokens = response.Usage.TotalTokens,
                    elapsedMs = response.Usage.ElapsedMs,
                    model = response.Usage.Model
                }
            });
        }

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
                return Ok(new { success = false, message = "[Aborted] Lệnh đã bị hủy bỏ an toàn." });
            }

            var result = await ExecuteToolAsync(req.FunctionName, req.FunctionArgs ?? "{}");

            _context.NhatKyHeThangs.Add(new NhatKyHeThong
            {
                HanhDong = "AIThucThi",
                MoTa = $"Thực thi: {req.FunctionName} => {(result.Success ? "OK" : "FAIL")}",
                NguoiThucHien = User.Identity?.Name,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                ThoiGian = DateTime.Now,
                ChiTietJson = JsonSerializer.Serialize(new { function = req.FunctionName, args = req.FunctionArgs, result = result.Message })
            });
            await _context.SaveChangesAsync();

            return Ok(new { success = result.Success, message = result.Message, data = result.Data });
        }

        // ==========================================
        // REPORT - Tạo báo cáo HTML thông minh
        // functionName xác định loại báo cáo cụ thể
        // ==========================================
        [HttpPost("report")]
        public async Task<IActionResult> GenerateReport([FromBody] ReportRequest req)
        {
            var fn = req.FunctionName?.Trim() ?? "";
            var prompt = req.Prompt?.ToLower() ?? "";
            var now = DateTime.Now;
            var sb = new StringBuilder();

            // Xác định loại báo cáo theo functionName trước, fallback sang prompt keyword
            bool isStoreList = fn == "DanhSachCuaHang"
                || prompt.Contains("danh sách") || prompt.Contains("danh sach")
                || prompt.Contains("list") || (prompt.Contains("cửa hàng") && !prompt.Contains("thống kê"));

            bool isNhatKy = fn == "XemNhatKy"
                || prompt.Contains("nhật ký") || prompt.Contains("nhat ky") || prompt.Contains("log");

            bool isSubscription = fn.Contains("DangKy") || fn.Contains("GiaHan")
                || prompt.Contains("đăng ký") || prompt.Contains("gia hạn") || prompt.Contains("gói");

            if (isStoreList)
            {
                await BuildStoreListReport(sb, now, req.Prompt ?? "");
            }
            else if (isNhatKy)
            {
                await BuildNhatKyReport(sb, now, req.Prompt ?? "");
            }
            else if (isSubscription)
            {
                await BuildSubscriptionReport(sb, now, req.Prompt ?? "");
            }
            else
            {
                // Mặc định: Thống kê tổng quan SaaS
                await BuildSaasOverviewReport(sb, now, req.Prompt ?? "");
            }

            return Ok(new { htmlReport = sb.ToString(), prompt = req.Prompt, generatedAt = now });
        }

        // ========== REPORT BUILDERS ==========

        private async Task BuildStoreListReport(StringBuilder sb, DateTime now, string promptText)
        {
            var stores = await _context.CuaHangs
                .OrderByDescending(c => c.NgayDangKy)
                .Select(c => new { c.Id, c.TenCuaHang, c.Email, c.TrangThai, c.GoiDichVu, c.NgayHetHan, c.NgayDangKy })
                .ToListAsync();

            int cntActive = stores.Count(s => s.TrangThai == "HoatDong");
            int cntTrial  = stores.Count(s => s.TrangThai == "DungThu");
            int cntLocked = stores.Count(s => s.TrangThai == "BiKhoa");
            int cntExpired = stores.Count(s => s.NgayHetHan < now);
            int cntExpiring = stores.Count(s => s.NgayHetHan >= now && s.NgayHetHan <= now.AddDays(7));

            sb.Append("<div style='font-family:Inter,sans-serif;background:#0f1117;color:#e4e4e7;padding:0;border-radius:12px;overflow:hidden'>");

            // Title bar
            sb.Append("<div style='background:linear-gradient(135deg,#1a1c23,#0f1117);padding:20px 24px;border-bottom:2px solid rgba(245,158,11,0.25)'>");
            sb.Append($"<div style='display:flex;align-items:center;justify-content:space-between'>");
            sb.Append($"<h2 style='color:#f59e0b;margin:0;font-size:1.25rem;font-weight:800'>🏪 Danh sách Cửa hàng</h2>");
            sb.Append($"<span style='background:#f59e0b22;color:#f59e0b;padding:5px 14px;border-radius:20px;font-size:0.8rem;font-weight:700;border:1px solid #f59e0b44'>Tổng: {stores.Count} cửa hàng</span>");
            sb.Append("</div>");
            sb.Append($"<p style='color:#6b7280;font-size:0.78rem;margin:6px 0 0'>Tạo lúc: {now:dd/MM/yyyy HH:mm} · Yêu cầu: {promptText}</p>");
            sb.Append("</div>");

            // Stat cards
            sb.Append("<div style='display:grid;grid-template-columns:repeat(5,1fr);gap:1px;background:rgba(255,255,255,0.04);border-bottom:1px solid rgba(255,255,255,0.06)'>");
            AppendStatCard(sb, cntActive.ToString(), "Hoạt động", "#22c55e");
            AppendStatCard(sb, cntTrial.ToString(), "Dùng thử", "#f59e0b");
            AppendStatCard(sb, cntLocked.ToString(), "Bị khóa", "#ef4444");
            AppendStatCard(sb, cntExpired.ToString(), "Hết hạn", "#8b5cf6");
            AppendStatCard(sb, cntExpiring.ToString(), "Sắp hết hạn (7 ngày)", "#06b6d4");
            sb.Append("</div>");

            // Table
            sb.Append("<div style='overflow-x:auto'>");
            sb.Append("<table style='width:100%;border-collapse:collapse;font-size:0.85rem'>");
            sb.Append("<thead><tr style='background:#f59e0b14;border-bottom:2px solid #f59e0b33'>");
            foreach (var h in new[] { "#", "Tên cửa hàng", "Email", "Gói", "Trạng thái", "Đăng ký", "Hết hạn" })
                sb.Append($"<th style='padding:11px 14px;text-align:left;color:#f59e0b;font-size:0.72rem;font-weight:700;text-transform:uppercase;white-space:nowrap'>{h}</th>");
            sb.Append("</tr></thead><tbody>");

            for (int i = 0; i < stores.Count; i++)
            {
                var c = stores[i];
                var bg = i % 2 == 0 ? "#111318" : "#0f1117";
                var sc = c.TrangThai switch { "HoatDong" => "#22c55e", "BiKhoa" => "#ef4444", "DungThu" => "#f59e0b", _ => "#6b7280" };
                var isExpired = c.NgayHetHan < now;
                var expColor = isExpired ? "#ef4444" : c.NgayHetHan <= now.AddDays(7) ? "#f59e0b" : "#6b7280";

                sb.Append($"<tr style='background:{bg};border-bottom:1px solid rgba(255,255,255,0.03)'>");
                sb.Append($"<td style='padding:9px 14px;color:#4b5563;font-size:0.75rem'>{c.Id}</td>");
                sb.Append($"<td style='padding:9px 14px;font-weight:600;color:#e4e4e7'>{HE(c.TenCuaHang)}</td>");
                sb.Append($"<td style='padding:9px 14px;color:#6b7280;font-size:0.82rem'>{(c.Email != null ? HE(c.Email) : "—")}</td>");
                sb.Append($"<td style='padding:9px 14px;color:#9ca3af;font-size:0.82rem'>{HE(c.GoiDichVu ?? "—")}</td>");
                sb.Append($"<td style='padding:9px 14px'><span style='background:{sc}22;color:{sc};padding:3px 10px;border-radius:6px;font-size:0.72rem;font-weight:700;white-space:nowrap'>{c.TrangThai}</span></td>");
                sb.Append($"<td style='padding:9px 14px;color:#6b7280;font-size:0.78rem'>{c.NgayDangKy:dd/MM/yyyy}</td>");
                sb.Append($"<td style='padding:9px 14px;color:{expColor};font-size:0.78rem;font-weight:{(isExpired ? "700" : "400")}'>{c.NgayHetHan:dd/MM/yyyy}{(isExpired ? " ⚠" : "")}</td>");
                sb.Append("</tr>");
            }
            sb.Append("</tbody></table></div></div>");
        }

        private async Task BuildNhatKyReport(StringBuilder sb, DateTime now, string promptText)
        {
            var logs = await _context.NhatKyHeThangs
                .OrderByDescending(n => n.ThoiGian)
                .Take(100)
                .Select(n => new { n.HanhDong, n.MoTa, n.NguoiThucHien, n.IpAddress, n.ThoiGian })
                .ToListAsync();

            sb.Append("<div style='font-family:Inter,sans-serif;background:#0f1117;color:#e4e4e7;padding:0;border-radius:12px;overflow:hidden'>");

            sb.Append("<div style='padding:20px 24px;border-bottom:2px solid rgba(245,158,11,0.2);background:linear-gradient(135deg,#1a1c23,#0f1117)'>");
            sb.Append($"<h2 style='color:#f59e0b;margin:0 0 4px;font-size:1.25rem;font-weight:800'>📋 Nhật ký hệ thống</h2>");
            sb.Append($"<p style='color:#6b7280;font-size:0.78rem;margin:0'>Tạo lúc: {now:dd/MM/yyyy HH:mm} · {logs.Count} bản ghi gần nhất</p>");
            sb.Append("</div>");

            // Count by action
            var grouped = logs.GroupBy(l => l.HanhDong).Select(g => new { Action = g.Key, Count = g.Count() }).OrderByDescending(x => x.Count).ToList();
            sb.Append("<div style='display:flex;gap:1px;background:rgba(255,255,255,0.04);border-bottom:1px solid rgba(255,255,255,0.06);overflow-x:auto'>");
            foreach (var g in grouped.Take(6))
            {
                var col = g.Action switch { "AIChat" => "#8b5cf6", "AIThucThi" => "#f59e0b", "Tao" => "#22c55e", "Sua" => "#3b82f6", "Xoa" => "#ef4444", _ => "#6b7280" };
                sb.Append($"<div style='flex:1;min-width:80px;padding:14px;text-align:center;background:#111318'>");
                sb.Append($"<div style='font-size:1.4rem;font-weight:800;color:{col}'>{g.Count}</div>");
                sb.Append($"<div style='font-size:0.7rem;color:#6b7280;margin-top:3px'>{g.Action}</div>");
                sb.Append("</div>");
            }
            sb.Append("</div>");

            sb.Append("<div style='overflow-x:auto'><table style='width:100%;border-collapse:collapse;font-size:0.83rem'>");
            sb.Append("<thead><tr style='background:#f59e0b14;border-bottom:2px solid #f59e0b33'>");
            foreach (var h in new[] { "Hành động", "Mô tả", "Người thực hiện", "IP", "Thời gian" })
                sb.Append($"<th style='padding:10px 14px;text-align:left;color:#f59e0b;font-size:0.72rem;font-weight:700;text-transform:uppercase;white-space:nowrap'>{h}</th>");
            sb.Append("</tr></thead><tbody>");

            for (int i = 0; i < logs.Count; i++)
            {
                var l = logs[i];
                var bg = i % 2 == 0 ? "#111318" : "#0f1117";
                var ac = l.HanhDong switch { "AIChat" => "#8b5cf6", "AIThucThi" => "#f59e0b", "Tao" => "#22c55e", "Sua" => "#3b82f6", "Xoa" => "#ef4444", _ => "#6b7280" };
                sb.Append($"<tr style='background:{bg};border-bottom:1px solid rgba(255,255,255,0.03)'>");
                sb.Append($"<td style='padding:8px 14px'><span style='background:{ac}22;color:{ac};padding:2px 9px;border-radius:5px;font-size:0.72rem;font-weight:700;white-space:nowrap'>{l.HanhDong}</span></td>");
                sb.Append($"<td style='padding:8px 14px;color:#9ca3af;max-width:300px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap'>{HE(l.MoTa ?? "")}</td>");
                sb.Append($"<td style='padding:8px 14px;color:#6b7280;font-size:0.8rem'>{HE(l.NguoiThucHien ?? "—")}</td>");
                sb.Append($"<td style='padding:8px 14px;color:#4b5563;font-size:0.75rem'><code>{l.IpAddress ?? "—"}</code></td>");
                sb.Append($"<td style='padding:8px 14px;color:#6b7280;font-size:0.78rem;white-space:nowrap'>{l.ThoiGian:dd/MM/yyyy HH:mm}</td>");
                sb.Append("</tr>");
            }
            sb.Append("</tbody></table></div></div>");
        }

        private async Task BuildSubscriptionReport(StringBuilder sb, DateTime now, string promptText)
        {
            var subs = await _context.LichSuDangKys
                .Include(l => l.CuaHang)
                .Include(l => l.GoiDichVu)
                .OrderByDescending(l => l.NgayTao)
                .Take(50)
                .ToListAsync();
            var subDtos = subs.Select(l => new {
                TenCuaHang = l.CuaHang != null ? l.CuaHang.TenCuaHang : "N/A",
                TenGoi = l.GoiDichVu != null ? l.GoiDichVu.TenGoi : $"Gói #{l.GoiDichVuId}",
                l.SoTienThanhToan, l.TrangThai, l.NgayBatDau, l.NgayKetThuc
            }).ToList();


            var totalRevenue = subs.Where(s => s.TrangThai == "DaThanhToan").Sum(s => s.SoTienThanhToan);

            sb.Append("<div style='font-family:Inter,sans-serif;background:#0f1117;color:#e4e4e7;padding:0;border-radius:12px;overflow:hidden'>");
            sb.Append("<div style='padding:20px 24px;border-bottom:2px solid rgba(245,158,11,0.2);background:linear-gradient(135deg,#1a1c23,#0f1117)'>");
            sb.Append($"<div style='display:flex;align-items:center;justify-content:space-between'>");
            sb.Append($"<h2 style='color:#f59e0b;margin:0;font-size:1.25rem;font-weight:800'>📦 Lịch sử đăng ký & Gia hạn</h2>");
            sb.Append($"<span style='background:#22c55e22;color:#22c55e;padding:5px 14px;border-radius:20px;font-size:0.8rem;font-weight:700;border:1px solid #22c55e44'>Doanh thu: {totalRevenue:N0}đ</span>");
            sb.Append("</div>");
            sb.Append($"<p style='color:#6b7280;font-size:0.78rem;margin:6px 0 0'>{subDtos.Count} giao dịch gần nhất · {now:dd/MM/yyyy HH:mm}</p>");
            sb.Append("</div>");

            // Stats
            var paid = subDtos.Count(s => s.TrangThai == "DaThanhToan");
            var pending = subDtos.Count(s => s.TrangThai == "ChoPhanHoi" || s.TrangThai == "DangXuLy");
            var cancelled = subDtos.Count(s => s.TrangThai == "DaHuy");
            sb.Append("<div style='display:grid;grid-template-columns:repeat(3,1fr);gap:1px;background:rgba(255,255,255,0.04);border-bottom:1px solid rgba(255,255,255,0.06)'>");
            AppendStatCard(sb, paid.ToString(), "Đã thanh toán", "#22c55e");
            AppendStatCard(sb, pending.ToString(), "Đang xử lý", "#f59e0b");
            AppendStatCard(sb, cancelled.ToString(), "Đã hủy", "#ef4444");
            sb.Append("</div>");

            sb.Append("<div style='overflow-x:auto'><table style='width:100%;border-collapse:collapse;font-size:0.83rem'>");
            sb.Append("<thead><tr style='background:#f59e0b14;border-bottom:2px solid #f59e0b33'>");
            foreach (var h in new[] { "Cửa hàng", "Gói", "Số tiền", "Trạng thái", "Bắt đầu", "Kết thúc" })
                sb.Append($"<th style='padding:10px 14px;text-align:left;color:#f59e0b;font-size:0.72rem;font-weight:700;text-transform:uppercase;white-space:nowrap'>{h}</th>");
            sb.Append("</tr></thead><tbody>");

            for (int i = 0; i < subDtos.Count; i++)
            {
                var s = subDtos[i];
                var bg = i % 2 == 0 ? "#111318" : "#0f1117";
                var sc = s.TrangThai switch { "DaThanhToan" => "#22c55e", "DaHuy" => "#ef4444", _ => "#f59e0b" };
                sb.Append($"<tr style='background:{bg};border-bottom:1px solid rgba(255,255,255,0.03)'>");
                sb.Append($"<td style='padding:9px 14px;font-weight:600;color:#e4e4e7'>{HE(s.TenCuaHang)}</td>");
                sb.Append($"<td style='padding:9px 14px;color:#9ca3af'>{HE(s.TenGoi)}</td>");
                sb.Append($"<td style='padding:9px 14px;color:#22c55e;font-weight:700'>{s.SoTienThanhToan:N0}đ</td>");
                sb.Append($"<td style='padding:9px 14px'><span style='background:{sc}22;color:{sc};padding:2px 9px;border-radius:5px;font-size:0.72rem;font-weight:700'>{s.TrangThai}</span></td>");
                sb.Append($"<td style='padding:9px 14px;color:#6b7280;font-size:0.78rem'>{s.NgayBatDau:dd/MM/yyyy}</td>");
                sb.Append($"<td style='padding:9px 14px;color:#6b7280;font-size:0.78rem'>{s.NgayKetThuc:dd/MM/yyyy}</td>");
                sb.Append("</tr>");
            }
            sb.Append("</tbody></table></div></div>");
        }

        private async Task BuildSaasOverviewReport(StringBuilder sb, DateTime now, string promptText)
        {
            var total   = await _context.CuaHangs.CountAsync();
            var active  = await _context.CuaHangs.CountAsync(c => c.TrangThai == "HoatDong");
            var trial   = await _context.CuaHangs.CountAsync(c => c.TrangThai == "DungThu");
            var locked  = await _context.CuaHangs.CountAsync(c => c.TrangThai == "BiKhoa");
            var chiDoc  = await _context.CuaHangs.CountAsync(c => c.TrangThai == "ChiDoc");
            var expiring7 = await _context.CuaHangs.CountAsync(c => c.TrangThai == "HoatDong" && c.NgayHetHan <= now.AddDays(7));
            var expiring30 = await _context.CuaHangs.CountAsync(c => c.NgayHetHan <= now.AddDays(30) && c.NgayHetHan > now);

            var revenueThisMonth = await _context.LichSuDangKys
                .Where(l => l.TrangThai == "DaThanhToan" && l.NgayThanhToan >= new DateTime(now.Year, now.Month, 1))
                .SumAsync(l => (decimal?)l.SoTienThanhToan) ?? 0;
            var revenueLastMonth = await _context.LichSuDangKys
                .Where(l => l.TrangThai == "DaThanhToan"
                    && l.NgayThanhToan >= new DateTime(now.Year, now.Month, 1).AddMonths(-1)
                    && l.NgayThanhToan < new DateTime(now.Year, now.Month, 1))
                .SumAsync(l => (decimal?)l.SoTienThanhToan) ?? 0;

            var newThisMonth = await _context.CuaHangs.CountAsync(c => c.NgayDangKy >= new DateTime(now.Year, now.Month, 1));

            sb.Append("<div style='font-family:Inter,sans-serif;background:#0f1117;color:#e4e4e7;padding:0;border-radius:12px;overflow:hidden'>");

            // Header
            sb.Append("<div style='padding:20px 24px;border-bottom:2px solid rgba(245,158,11,0.2);background:linear-gradient(135deg,#1a1c23,#0f1117)'>");
            sb.Append($"<h2 style='color:#f59e0b;margin:0 0 4px;font-size:1.25rem;font-weight:800'>📊 Thống kê tổng quan SaaS</h2>");
            sb.Append($"<p style='color:#6b7280;font-size:0.78rem;margin:0'>Tạo lúc: {now:dd/MM/yyyy HH:mm:ss} · {promptText}</p>");
            sb.Append("</div>");

            // Main stats
            sb.Append("<div style='display:grid;grid-template-columns:repeat(4,1fr);gap:1px;background:rgba(255,255,255,0.04);border-bottom:1px solid rgba(255,255,255,0.06)'>");
            AppendStatCard(sb, active.ToString(), "Đang hoạt động", "#22c55e");
            AppendStatCard(sb, trial.ToString(), "Dùng thử", "#f59e0b");
            AppendStatCard(sb, locked.ToString(), "Bị khóa", "#ef4444");
            AppendStatCard(sb, total.ToString(), "Tổng cửa hàng", "#3b82f6");
            sb.Append("</div>");

            // Revenue section
            sb.Append("<div style='padding:16px 24px;border-bottom:1px solid rgba(255,255,255,0.06);background:#111318'>");
            sb.Append("<h3 style='color:#9ca3af;font-size:0.75rem;font-weight:700;text-transform:uppercase;letter-spacing:0.5px;margin:0 0 12px'>💰 Doanh thu</h3>");
            sb.Append("<div style='display:grid;grid-template-columns:repeat(3,1fr);gap:12px'>");

            var revChange = revenueLastMonth > 0 ? ((revenueThisMonth - revenueLastMonth) / revenueLastMonth * 100) : 0;
            var revColor = revChange >= 0 ? "#22c55e" : "#ef4444";
            var revArrow = revChange >= 0 ? "↑" : "↓";

            sb.Append($"<div style='background:#0f1117;border:1px solid rgba(34,197,94,0.2);border-radius:10px;padding:16px'>");
            sb.Append($"<div style='color:#6b7280;font-size:0.72rem;margin-bottom:6px'>Tháng này ({now:MM/yyyy})</div>");
            sb.Append($"<div style='font-size:1.4rem;font-weight:800;color:#22c55e'>{revenueThisMonth:N0}đ</div>");
            sb.Append($"<div style='color:{revColor};font-size:0.75rem;margin-top:4px'>{revArrow} {Math.Abs(revChange):F1}% so với tháng trước</div>");
            sb.Append("</div>");

            sb.Append($"<div style='background:#0f1117;border:1px solid rgba(107,114,128,0.2);border-radius:10px;padding:16px'>");
            sb.Append($"<div style='color:#6b7280;font-size:0.72rem;margin-bottom:6px'>Tháng trước</div>");
            sb.Append($"<div style='font-size:1.4rem;font-weight:800;color:#9ca3af'>{revenueLastMonth:N0}đ</div>");
            sb.Append("</div>");

            sb.Append($"<div style='background:#0f1117;border:1px solid rgba(59,130,246,0.2);border-radius:10px;padding:16px'>");
            sb.Append($"<div style='color:#6b7280;font-size:0.72rem;margin-bottom:6px'>Cửa hàng mới tháng này</div>");
            sb.Append($"<div style='font-size:1.4rem;font-weight:800;color:#3b82f6'>{newThisMonth}</div>");
            sb.Append("</div>");
            sb.Append("</div></div>");

            // Alerts
            sb.Append("<div style='padding:16px 24px'>");
            sb.Append("<h3 style='color:#9ca3af;font-size:0.75rem;font-weight:700;text-transform:uppercase;letter-spacing:0.5px;margin:0 0 12px'>⚠️ Cảnh báo cần chú ý</h3>");
            sb.Append("<div style='display:flex;flex-direction:column;gap:8px'>");

            if (expiring7 > 0)
                AppendAlert(sb, "🔴", $"{expiring7} cửa hàng sẽ hết hạn trong 7 ngày tới - cần liên hệ gia hạn ngay", "#ef4444");
            if (expiring30 > 0)
                AppendAlert(sb, "🟡", $"{expiring30} cửa hàng sẽ hết hạn trong 30 ngày - theo dõi và nhắc nhở", "#f59e0b");
            if (locked > 0)
                AppendAlert(sb, "🔒", $"{locked} cửa hàng đang bị khóa - kiểm tra lý do và xem xét mở khóa", "#8b5cf6");
            if (chiDoc > 0)
                AppendAlert(sb, "📖", $"{chiDoc} cửa hàng ở chế độ chỉ đọc - đã hết hạn nhưng chưa được xử lý", "#6b7280");
            if (expiring7 == 0 && expiring30 == 0 && locked == 0)
                AppendAlert(sb, "✅", "Hệ thống đang hoạt động ổn định, không có cảnh báo nghiêm trọng", "#22c55e");

            sb.Append("</div></div></div>");
        }

        private static void AppendStatCard(StringBuilder sb, string value, string label, string color)
        {
            sb.Append($"<div style='padding:16px;text-align:center;background:#111318'>");
            sb.Append($"<div style='font-size:1.6rem;font-weight:800;color:{color}'>{value}</div>");
            sb.Append($"<div style='font-size:0.72rem;color:#6b7280;margin-top:4px;white-space:nowrap'>{label}</div>");
            sb.Append("</div>");
        }

        private static void AppendAlert(StringBuilder sb, string icon, string text, string color)
        {
            sb.Append($"<div style='display:flex;align-items:center;gap:10px;padding:10px 14px;background:{color}11;border:1px solid {color}33;border-radius:8px;font-size:0.83rem'>");
            sb.Append($"<span style='font-size:1rem'>{icon}</span>");
            sb.Append($"<span style='color:#d1d5db'>{text}</span>");
            sb.Append("</div>");
        }

        // HTML encode helper - use UTF-8 directly, only escape special HTML chars
        private static string HE(string s) => System.Web.HttpUtility.HtmlEncode(s);

        [HttpDelete("session/{sessionId}")]
        public IActionResult ClearSession(string sessionId)
        {
            lock (_lock) { _sessions.Remove(sessionId); }
            return Ok(new { message = "Session đã được xóa." });
        }

        [HttpGet("tools")]
        public IActionResult GetTools() =>
            Ok(GeminiAIService.AvailableTools.Select(t => new { t.Name, t.Description, t.RiskLevel }));

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
                    "ThemGoiSaaS" => await ToolThemGoiSaaS(
                        args.TryGetProperty("tenGoi", out var tg) ? tg.GetString() : null,
                        args.TryGetProperty("maGoi", out var mg) ? mg.GetString() : null,
                        args.TryGetProperty("soThang", out var so) ? so.GetInt32() : 0,
                        args.TryGetProperty("giaThang", out var gt) ? gt.GetDecimal() : 0,
                        args.TryGetProperty("tongGia", out var tgi) ? tgi.GetDecimal() : 0,
                        args.TryGetProperty("gioiHanHoaDon", out var ghd) ? ghd.GetInt32() : 0,
                        args.TryGetProperty("gioiHanNhanVien", out var gnv) ? gnv.GetInt32() : 0,
                        args.TryGetProperty("moTa", out var mt) ? mt.GetString() : null),
                    "ThietLapHeThong" => await ToolThietLap(
                        args.TryGetProperty("key", out var k) ? k.GetString() : null,
                        args.TryGetProperty("value", out var v) ? v.GetString() : null),
                    "XuatBaoCaoAI" => new ToolResult { Success = true, Message = "Báo cáo AI đã được mở." },
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
            return new ToolResult { Success = true, Message = "Thống kê SaaS hiện tại đã sẵn sàng.", Data = data };
        }

        private async Task<ToolResult> ToolDanhSach(string? trangThai, bool sapHetHan)
        {
            var q = _context.CuaHangs.AsQueryable();
            if (!string.IsNullOrEmpty(trangThai)) q = q.Where(c => c.TrangThai == trangThai);
            if (sapHetHan) q = q.Where(c => c.NgayHetHan <= DateTime.Now.AddDays(7));
            var list = await q.OrderByDescending(c => c.NgayDangKy)
                .Select(c => new { c.Id, c.TenCuaHang, c.Email, c.TrangThai, c.GoiDichVu, c.NgayHetHan })
                .ToListAsync();
            return new ToolResult { Success = true, Message = $"Tổng {list.Count} cửa hàng.", Data = list };
        }

        private async Task<ToolResult> ToolKhoa(int id, string lyDo)
        {
            var ch = await _context.CuaHangs.FindAsync(id);
            if (ch == null) return new ToolResult { Success = false, Message = $"Không tìm thấy cửa hàng #{id}" };
            ch.TrangThai = "BiKhoa"; ch.GhiChu = $"[AI Lock] {lyDo}";
            await _context.SaveChangesAsync();
            return new ToolResult { Success = true, Message = $"Đã khóa: [{ch.TenCuaHang}]" };
        }

        private async Task<ToolResult> ToolMoKhoa(int id)
        {
            var ch = await _context.CuaHangs.FindAsync(id);
            if (ch == null) return new ToolResult { Success = false, Message = $"Không tìm thấy cửa hàng #{id}" };
            ch.TrangThai = ch.NgayHetHan > DateTime.Now ? "HoatDong" : "ChiDoc";
            await _context.SaveChangesAsync();
            return new ToolResult { Success = true, Message = $"Đã mở khóa: [{ch.TenCuaHang}] => {ch.TrangThai}" };
        }

        private async Task<ToolResult> ToolThemGoiSaaS(string? tenGoi, string? maGoi, int soThang, decimal giaThang, decimal tongGia, int gioiHanHoaDon, int gioiHanNhanVien, string? moTa)
        {
            if (string.IsNullOrEmpty(tenGoi) || string.IsNullOrEmpty(maGoi) || soThang <= 0)
                return new ToolResult { Success = false, Message = "Tên gói, mã gói và số tháng không hợp lệ." };
            var existing = await _context.GoiDichVus.FirstOrDefaultAsync(g => g.MaGoi == maGoi);
            if (existing != null) return new ToolResult { Success = false, Message = $"Gói '{maGoi}' đã tồn tại." };
            var p = new POS36.Api.Models.GoiDichVu
            {
                TenGoi = tenGoi, MaGoi = maGoi, SoThang = soThang,
                GiaThang = giaThang, TongGia = tongGia > 0 ? tongGia : giaThang * soThang,
                GioiHanHoaDon = gioiHanHoaDon, GioiHanNhanVien = gioiHanNhanVien,
                MoTa = moTa, IsActive = true,
                ThuTuHienThi = await _context.GoiDichVus.CountAsync() + 1
            };
            _context.GoiDichVus.Add(p);
            await _context.SaveChangesAsync();
            return new ToolResult { Success = true, Message = $"Đã tạo gói mới: [{p.TenGoi}] ({p.MaGoi}) - {p.SoThang} tháng." };
        }

        private async Task<ToolResult> ToolThietLap(string? key, string? value)
        {
            if (string.IsNullOrEmpty(key) || value == null) return new ToolResult { Success = false, Message = "Thiếu Key hoặc Value." };
            var config = await _context.CauHinhHeThangs.FirstOrDefaultAsync(c => c.MaKey == key);
            if (config != null) { config.GiaTri = value; config.NgayCapNhat = DateTime.Now; config.NguoiCapNhat = User.Identity?.Name ?? "AI"; }
            else _context.CauHinhHeThangs.Add(new CauHinhHeThong { MaKey = key, GiaTri = value, NhomCauHinh = "System", NguoiCapNhat = User.Identity?.Name ?? "AI", NgayCapNhat = DateTime.Now });
            await _context.SaveChangesAsync();
            return new ToolResult { Success = true, Message = $"Đã cập nhật: {key} = {value}" };
        }

        private async Task<ToolResult> ToolGiaHan(int id, int soThang, string? goiMoi)
        {
            var ch = await _context.CuaHangs.FindAsync(id);
            if (ch == null) return new ToolResult { Success = false, Message = $"Không tìm thấy cửa hàng #{id}" };
            ch.NgayHetHan = (ch.NgayHetHan > DateTime.Now ? ch.NgayHetHan : DateTime.Now).AddMonths(soThang);
            ch.TrangThai = "HoatDong";
            if (!string.IsNullOrEmpty(goiMoi)) ch.GoiDichVu = goiMoi;
            await _context.SaveChangesAsync();
            return new ToolResult { Success = true, Message = $"Đã gia hạn [{ch.TenCuaHang}] +{soThang} tháng => {ch.NgayHetHan:dd/MM/yyyy}" };
        }

        private async Task<ToolResult> ToolThongBao(int cuaHangId, string tieuDe, string noiDung, string loai)
        {
            _context.ThongBaoHeThongs.Add(new ThongBaoHeThong { TieuDe = tieuDe, NoiDung = noiDung, LoaiThongBao = loai, CuaHangId = cuaHangId == 0 ? null : cuaHangId, NgayTao = DateTime.Now, DaDoc = false });
            await _context.SaveChangesAsync();
            return new ToolResult { Success = true, Message = $"Đã gửi thông báo đến {(cuaHangId == 0 ? "tất cả" : $"quán #{cuaHangId}")}" };
        }

        private async Task<ToolResult> ToolNhatKy(string? tuNgay, string? hanhDong)
        {
            var q = _context.NhatKyHeThangs.AsQueryable();
            if (DateTime.TryParse(tuNgay, out var from)) q = q.Where(n => n.ThoiGian >= from);
            if (!string.IsNullOrEmpty(hanhDong)) q = q.Where(n => n.HanhDong == hanhDong);
            var logs = await q.OrderByDescending(n => n.ThoiGian).Take(100)
                .Select(n => new { n.HanhDong, n.MoTa, n.NguoiThucHien, n.ThoiGian }).ToListAsync();
            return new ToolResult { Success = true, Message = $"{logs.Count} bản ghi nhật ký.", Data = logs };
        }

        private static string TruncateStr(string s, int max) => s.Length <= max ? s : s[..max] + "...";
    }

    public class ChatRequest { public string Prompt { get; set; } = ""; public string? SessionId { get; set; } public string? ModelId { get; set; } public string Mode { get; set; } = "agent"; }
    public class ReportRequest { public string? Prompt { get; set; } public string? FunctionName { get; set; } }
    public class ConfirmRequest { public bool Confirmed { get; set; } public string FunctionName { get; set; } = ""; public string? FunctionArgs { get; set; } }
    public class ToolResult { public bool Success { get; set; } public string Message { get; set; } = ""; public object? Data { get; set; } }
}