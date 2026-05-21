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

        // ==========================================
        // 0. DANH SACH MODELS
        // ==========================================
        [HttpGet("models")]
        public async Task<IActionResult> GetModels()
        {
            var models = await _gemini.GetModelsAsync();
            return Ok(models);
        }

        // ==========================================
        // 1. CHAT
        // ==========================================
        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Prompt))
                return BadRequest(new { error = "Prompt khong duoc de trong!" });

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
                    Text = response.Type == GeminiResponseType.Text
                        ? response.Text
                        : $"[FunctionCall: {response.FunctionName}]"
                });
                if (_sessions[sessionId].Count > 40)
                    _sessions[sessionId].RemoveRange(0, 2);
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

        // ==========================================
        // 2. CONFIRM
        // ==========================================
        [HttpPost("confirm")]
        public async Task<IActionResult> Confirm([FromBody] ConfirmRequest req)
        {
            if (!req.Confirmed)
            {
                _context.NhatKyHeThangs.Add(new NhatKyHeThong
                {
                    HanhDong = "AIHuyLenh",
                    MoTa = $"Huy lenh AI: {req.FunctionName}",
                    NguoiThucHien = User.Identity?.Name,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    ThoiGian = DateTime.Now
                });
                await _context.SaveChangesAsync();
                return Ok(new { success = false, message = "[Aborted] Lenh da bi huy bo an toan." });
            }

            var result = await ExecuteToolAsync(req.FunctionName, req.FunctionArgs ?? "{}");

            _context.NhatKyHeThangs.Add(new NhatKyHeThong
            {
                HanhDong = "AIThucThi",
                MoTa = $"Thuc thi: {req.FunctionName} => {(result.Success ? "OK" : "FAIL")}",
                NguoiThucHien = User.Identity?.Name,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                ThoiGian = DateTime.Now,
                ChiTietJson = JsonSerializer.Serialize(new { function = req.FunctionName, args = req.FunctionArgs, result = result.Message })
            });
            await _context.SaveChangesAsync();

            return Ok(new { success = result.Success, message = result.Message, data = result.Data });
        }

        // ==========================================
        // 3. REPORT - Tao bao cao HTML tu du lieu thuc
        // ==========================================
        [HttpPost("report")]
        public async Task<IActionResult> GenerateReport([FromBody] ReportRequest req)
        {
            var prompt = req.Prompt?.ToLower() ?? "";
            var now = DateTime.Now;
            var sb = new StringBuilder();

            bool isStoreReport = prompt.Contains("cua hang") || prompt.Contains("c\u1eeda h\u00e0ng")
                || prompt.Contains("store") || prompt.Contains("danh sach") || prompt.Contains("danh s\u00e1ch");

            if (isStoreReport)
            {
                var stores = await _context.CuaHangs
                    .OrderByDescending(c => c.NgayDangKy)
                    .Select(c => new { c.Id, c.TenCuaHang, c.Email, c.TrangThai, c.GoiDichVu, c.NgayHetHan, c.NgayDangKy })
                    .ToListAsync();

                int cntActive = stores.Count(s => s.TrangThai == "HoatDong");
                int cntTrial = stores.Count(s => s.TrangThai == "DungThu");
                int cntLocked = stores.Count(s => s.TrangThai == "BiKhoa");
                int cntExpired = stores.Count(s => s.NgayHetHan < now);

                sb.Append("<div style='font-family:Inter,Arial,sans-serif;background:#0f1117;color:#e4e4e7;padding:20px;border-radius:12px'>");
                sb.Append("<div style='display:flex;align-items:center;justify-content:space-between;margin-bottom:20px'>");
                sb.Append("<h2 style='color:#f59e0b;margin:0;font-size:1.3rem'>&#x1F3EA; Danh s&aacute;ch C&#x1EED;a h&agrave;ng</h2>");
                sb.Append($"<span style='background:#f59e0b22;color:#f59e0b;padding:6px 14px;border-radius:20px;font-size:0.82rem;font-weight:700'>T&#x1ED5;ng: {stores.Count} c&#x1EED;a h&agrave;ng</span>");
                sb.Append("</div>");
                sb.Append($"<p style='color:#6b7280;font-size:0.8rem;margin-bottom:16px'>T&#x1EA1;o l&uacute;c: {now:dd/MM/yyyy HH:mm}</p>");
                // Stats
                sb.Append("<div style='display:grid;grid-template-columns:repeat(4,1fr);gap:12px;margin-bottom:20px'>");
                sb.Append($"<div style='background:#22c55e22;border:1px solid #22c55e44;border-radius:10px;padding:14px;text-align:center'><div style='font-size:1.5rem;font-weight:800;color:#22c55e'>{cntActive}</div><div style='font-size:0.75rem;color:#6b7280;margin-top:4px'>Ho&#x1EA1;t &dagger;&#x1ED9;ng</div></div>");
                sb.Append($"<div style='background:#f59e0b22;border:1px solid #f59e0b44;border-radius:10px;padding:14px;text-align:center'><div style='font-size:1.5rem;font-weight:800;color:#f59e0b'>{cntTrial}</div><div style='font-size:0.75rem;color:#6b7280;margin-top:4px'>D&ugrave;ng th&#x1EED;</div></div>");
                sb.Append($"<div style='background:#ef444422;border:1px solid #ef444444;border-radius:10px;padding:14px;text-align:center'><div style='font-size:1.5rem;font-weight:800;color:#ef4444'>{cntLocked}</div><div style='font-size:0.75rem;color:#6b7280;margin-top:4px'>B&#x1ECB; kh&oacute;a</div></div>");
                sb.Append($"<div style='background:#8b5cf622;border:1px solid #8b5cf644;border-radius:10px;padding:14px;text-align:center'><div style='font-size:1.5rem;font-weight:800;color:#8b5cf6'>{cntExpired}</div><div style='font-size:0.75rem;color:#6b7280;margin-top:4px'>H&#x1EBF;t h&#x1EA1;n</div></div>");
                sb.Append("</div>");
                // Table
                sb.Append("<table style='width:100%;border-collapse:collapse'><thead>");
                sb.Append("<tr style='background:#f59e0b22;border-bottom:2px solid #f59e0b44'>");
                sb.Append("<th style='padding:10px 12px;text-align:left;color:#f59e0b;font-size:0.75rem'>#ID</th>");
                sb.Append("<th style='padding:10px 12px;text-align:left;color:#f59e0b;font-size:0.75rem'>T&ecirc;n c&#x1EED;a h&agrave;ng</th>");
                sb.Append("<th style='padding:10px 12px;text-align:left;color:#f59e0b;font-size:0.75rem'>Email</th>");
                sb.Append("<th style='padding:10px 12px;text-align:left;color:#f59e0b;font-size:0.75rem'>Tr&#x1EA1;ng th&aacute;i</th>");
                sb.Append("<th style='padding:10px 12px;text-align:left;color:#f59e0b;font-size:0.75rem'>G&oacute;i d&#x1ECB;ch v&#x1EE5;</th>");
                sb.Append("<th style='padding:10px 12px;text-align:left;color:#f59e0b;font-size:0.75rem'>H&#x1EBF;t h&#x1EA1;n</th>");
                sb.Append("</tr></thead><tbody>");

                for (int i = 0; i < stores.Count; i++)
                {
                    var c = stores[i];
                    var bg = i % 2 == 0 ? "#1a1c23" : "#16181d";
                    var sc = c.TrangThai == "HoatDong" ? "#22c55e" : c.TrangThai == "BiKhoa" ? "#ef4444" : "#f59e0b";
                    var expStr = c.NgayHetHan.ToString("dd/MM/yyyy");
                    var expColor = c.NgayHetHan < now ? "#ef4444" : "#9ca3af";
                    sb.Append($"<tr style='background:{bg}'>");
                    sb.Append($"<td style='padding:10px 12px;color:#9ca3af;font-size:0.78rem'>{c.Id}</td>");
                    sb.Append($"<td style='padding:10px 12px;font-weight:600;color:#e4e4e7'>{System.Web.HttpUtility.HtmlEncode(c.TenCuaHang)}</td>");
                    sb.Append($"<td style='padding:10px 12px;color:#9ca3af;font-size:0.82rem'>{(c.Email != null ? System.Web.HttpUtility.HtmlEncode(c.Email) : "-")}</td>");
                    sb.Append($"<td style='padding:10px 12px'><span style='background:{sc}22;color:{sc};padding:3px 10px;border-radius:6px;font-size:0.75rem;font-weight:700'>{c.TrangThai}</span></td>");
                    sb.Append($"<td style='padding:10px 12px;color:#9ca3af;font-size:0.82rem'>{(c.GoiDichVu ?? "-")}</td>");
                    sb.Append($"<td style='padding:10px 12px;color:{expColor};font-size:0.82rem'>{expStr}</td>");
                    sb.Append("</tr>");
                }
                sb.Append("</tbody></table></div>");
            }
            else
            {
                var totalStores = await _context.CuaHangs.CountAsync();
                var active = await _context.CuaHangs.CountAsync(c => c.TrangThai == "HoatDong");
                var trial = await _context.CuaHangs.CountAsync(c => c.TrangThai == "DungThu");
                var locked = await _context.CuaHangs.CountAsync(c => c.TrangThai == "BiKhoa");
                var expiring = await _context.CuaHangs.CountAsync(c => c.TrangThai == "HoatDong" && c.NgayHetHan <= now.AddDays(7));
                var revenue = await _context.LichSuDangKys
                    .Where(l => l.TrangThai == "DaThanhToan" && l.NgayThanhToan >= new DateTime(now.Year, now.Month, 1))
                    .SumAsync(l => (decimal?)l.SoTienThanhToan) ?? 0;

                sb.Append("<div style='font-family:Inter,Arial,sans-serif;background:#0f1117;color:#e4e4e7;padding:20px;border-radius:12px'>");
                sb.Append("<h2 style='color:#f59e0b;margin:0 0 6px;font-size:1.3rem'>Th&#x1ED1;ng k&ecirc; t&#x1ED5;ng quan SaaS</h2>");
                sb.Append($"<p style='color:#6b7280;font-size:0.8rem;margin-bottom:20px'>{now:dd/MM/yyyy HH:mm}</p>");
                sb.Append("<div style='display:grid;grid-template-columns:repeat(3,1fr);gap:14px'>");
                sb.Append($"<div style='background:#22c55e22;border:1px solid #22c55e44;border-radius:10px;padding:16px;text-align:center'><div style='font-size:2rem;font-weight:800;color:#22c55e'>{active}</div><div style='color:#6b7280;font-size:0.8rem;margin-top:4px'>&#x110;ang ho&#x1EA1;t &dagger;&#x1ED9;ng</div></div>");
                sb.Append($"<div style='background:#f59e0b22;border:1px solid #f59e0b44;border-radius:10px;padding:16px;text-align:center'><div style='font-size:2rem;font-weight:800;color:#f59e0b'>{trial}</div><div style='color:#6b7280;font-size:0.8rem;margin-top:4px'>D&ugrave;ng th&#x1EED;</div></div>");
                sb.Append($"<div style='background:#ef444422;border:1px solid #ef444444;border-radius:10px;padding:16px;text-align:center'><div style='font-size:2rem;font-weight:800;color:#ef4444'>{locked}</div><div style='color:#6b7280;font-size:0.8rem;margin-top:4px'>B&#x1ECB; kh&oacute;a</div></div>");
                sb.Append($"<div style='background:#3b82f622;border:1px solid #3b82f644;border-radius:10px;padding:16px;text-align:center'><div style='font-size:2rem;font-weight:800;color:#3b82f6'>{totalStores}</div><div style='color:#6b7280;font-size:0.8rem;margin-top:4px'>T&#x1ED5;ng c&#x1EED;a h&agrave;ng</div></div>");
                sb.Append($"<div style='background:#8b5cf622;border:1px solid #8b5cf644;border-radius:10px;padding:16px;text-align:center'><div style='font-size:2rem;font-weight:800;color:#8b5cf6'>{expiring}</div><div style='color:#6b7280;font-size:0.8rem;margin-top:4px'>S&#x1EAF;p h&#x1EBF;t h&#x1EA1;n (7 ng&agrave;y)</div></div>");
                sb.Append($"<div style='background:#06b6d422;border:1px solid #06b6d444;border-radius:10px;padding:16px;text-align:center'><div style='font-size:1.4rem;font-weight:800;color:#06b6d4'>{revenue:N0}d</div><div style='color:#6b7280;font-size:0.8rem;margin-top:4px'>Doanh thu th&aacute;ng n&agrave;y</div></div>");
                sb.Append("</div></div>");
            }

            return Ok(new { htmlReport = sb.ToString(), prompt = req.Prompt, generatedAt = now });
        }

        // ==========================================
        // 4. XOA SESSION
        // ==========================================
        [HttpDelete("session/{sessionId}")]
        public IActionResult ClearSession(string sessionId)
        {
            lock (_lock) { _sessions.Remove(sessionId); }
            return Ok(new { message = "Session da duoc xoa." });
        }

        // ==========================================
        // 5. DANH SACH TOOLS
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
                    "XuatBaoCaoAI" => new ToolResult { Success = true, Message = "Bao cao AI da duoc mo." },
                    _ => new ToolResult { Success = false, Message = $"Tool '{fn}' khong duoc ho tro." }
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Tool execution error: {Fn}", fn);
                return new ToolResult { Success = false, Message = $"Loi thuc thi: {ex.Message}" };
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
            return new ToolResult { Success = true, Message = "Thong ke SaaS hien tai da san sang.", Data = data };
        }

        private async Task<ToolResult> ToolDanhSach(string? trangThai, bool sapHetHan)
        {
            var q = _context.CuaHangs.AsQueryable();
            if (!string.IsNullOrEmpty(trangThai)) q = q.Where(c => c.TrangThai == trangThai);
            if (sapHetHan) q = q.Where(c => c.NgayHetHan <= DateTime.Now.AddDays(7));
            // No limit - return all stores
            var list = await q
                .OrderByDescending(c => c.NgayDangKy)
                .Select(c => new { c.Id, c.TenCuaHang, c.Email, c.TrangThai, c.GoiDichVu, c.NgayHetHan })
                .ToListAsync();
            return new ToolResult { Success = true, Message = $"Tong {list.Count} cua hang.", Data = list };
        }

        private async Task<ToolResult> ToolKhoa(int id, string lyDo)
        {
            var ch = await _context.CuaHangs.FindAsync(id);
            if (ch == null) return new ToolResult { Success = false, Message = $"Khong tim thay cua hang #{id}" };
            ch.TrangThai = "BiKhoa"; ch.GhiChu = $"[AI Lock] {lyDo}";
            await _context.SaveChangesAsync();
            return new ToolResult { Success = true, Message = $"Da khoa: [{ch.TenCuaHang}]" };
        }

        private async Task<ToolResult> ToolMoKhoa(int id)
        {
            var ch = await _context.CuaHangs.FindAsync(id);
            if (ch == null) return new ToolResult { Success = false, Message = $"Khong tim thay cua hang #{id}" };
            ch.TrangThai = ch.NgayHetHan > DateTime.Now ? "HoatDong" : "ChiDoc";
            await _context.SaveChangesAsync();
            return new ToolResult { Success = true, Message = $"Da mo khoa: [{ch.TenCuaHang}] => {ch.TrangThai}" };
        }

        private async Task<ToolResult> ToolThemGoiSaaS(string? tenGoi, string? maGoi, int soThang, decimal giaThang, decimal tongGia, int gioiHanHoaDon, int gioiHanNhanVien, string? moTa)
        {
            if (string.IsNullOrEmpty(tenGoi) || string.IsNullOrEmpty(maGoi) || soThang <= 0)
                return new ToolResult { Success = false, Message = "Ten goi, ma goi va so thang khong hop le." };

            var existing = await _context.GoiDichVus.FirstOrDefaultAsync(g => g.MaGoi == maGoi);
            if (existing != null) return new ToolResult { Success = false, Message = $"Goi '{maGoi}' da ton tai." };

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
            return new ToolResult { Success = true, Message = $"Da tao goi moi: [{p.TenGoi}] ({p.MaGoi}) - {p.SoThang} thang." };
        }

        private async Task<ToolResult> ToolThietLap(string? key, string? value)
        {
            if (string.IsNullOrEmpty(key) || value == null)
                return new ToolResult { Success = false, Message = "Thieu tham so Key hoac Value." };

            var config = await _context.CauHinhHeThangs.FirstOrDefaultAsync(c => c.MaKey == key);
            if (config != null) { config.GiaTri = value; config.NgayCapNhat = DateTime.Now; config.NguoiCapNhat = User.Identity?.Name ?? "AI Agent"; }
            else
            {
                _context.CauHinhHeThangs.Add(new CauHinhHeThong
                {
                    MaKey = key, GiaTri = value, NhomCauHinh = "System",
                    NguoiCapNhat = User.Identity?.Name ?? "AI Agent", NgayCapNhat = DateTime.Now
                });
            }
            await _context.SaveChangesAsync();
            return new ToolResult { Success = true, Message = $"Da cap nhat cau hinh: {key} = {value}" };
        }

        private async Task<ToolResult> ToolGiaHan(int id, int soThang, string? goiMoi)
        {
            var ch = await _context.CuaHangs.FindAsync(id);
            if (ch == null) return new ToolResult { Success = false, Message = $"Khong tim thay cua hang #{id}" };
            ch.NgayHetHan = (ch.NgayHetHan > DateTime.Now ? ch.NgayHetHan : DateTime.Now).AddMonths(soThang);
            ch.TrangThai = "HoatDong";
            if (!string.IsNullOrEmpty(goiMoi)) ch.GoiDichVu = goiMoi;
            await _context.SaveChangesAsync();
            return new ToolResult { Success = true, Message = $"Da gia han [{ch.TenCuaHang}] +{soThang} thang => {ch.NgayHetHan:dd/MM/yyyy}" };
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
            return new ToolResult { Success = true, Message = $"Da gui thong bao den {(cuaHangId == 0 ? "tat ca" : $"quan #{cuaHangId}")}" };
        }

        private async Task<ToolResult> ToolNhatKy(string? tuNgay, string? hanhDong)
        {
            var q = _context.NhatKyHeThangs.AsQueryable();
            if (DateTime.TryParse(tuNgay, out var from)) q = q.Where(n => n.ThoiGian >= from);
            if (!string.IsNullOrEmpty(hanhDong)) q = q.Where(n => n.HanhDong == hanhDong);
            var logs = await q.OrderByDescending(n => n.ThoiGian).Take(100)
                .Select(n => new { n.HanhDong, n.MoTa, n.NguoiThucHien, n.ThoiGian }).ToListAsync();
            return new ToolResult { Success = true, Message = $"{logs.Count} ban ghi nhat ky.", Data = logs };
        }

        private static string TruncateStr(string s, int max) => s.Length <= max ? s : s[..max] + "...";
    }

    public class ChatRequest
    {
        public string Prompt { get; set; } = "";
        public string? SessionId { get; set; }
        public string? ModelId { get; set; }
        public string Mode { get; set; } = "agent";
    }
    public class ReportRequest { public string? Prompt { get; set; } }
    public class ConfirmRequest { public bool Confirmed { get; set; } public string FunctionName { get; set; } = ""; public string? FunctionArgs { get; set; } }
    public class ToolResult { public bool Success { get; set; } public string Message { get; set; } = ""; public object? Data { get; set; } }
}