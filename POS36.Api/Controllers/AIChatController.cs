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
    [Authorize]
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

        // ── GET /api/AIChat/models ──────────────────────────────────────
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("models")]
        public async Task<IActionResult> GetModels()
        {
            var models = await _gemini.GetModelsAsync();
            return Ok(models);
        }

        // ── POST /api/AIChat/chat ───────────────────────────────────────
        [Authorize(Roles = "SuperAdmin")]
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
                    Text = response.Type == GeminiResponseType.Text
                        ? response.Text
                        : $"[FunctionCall: {response.FunctionName}]"
                });
                if (_sessions[sessionId].Count > 40) _sessions[sessionId].RemoveRange(0, 2);
            }

            _context.NhatKyHeThangs.Add(new NhatKyHeThong
            {
                HanhDong = "AIChat",
                MoTa = $"Prompt: \"{Truncate(req.Prompt, 100)}\" => {response.Type}",
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

        // ── POST /api/AIChat/confirm ────────────────────────────────────
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("confirm")]
        public async Task<IActionResult> Confirm([FromBody] ConfirmRequest req)
        {
            if (!req.Confirmed)
            {
                _context.NhatKyHeThangs.Add(new NhatKyHeThong
                {
                    HanhDong = "AIHuyLenh",
                    MoTa = $"Hủy lệnh: {req.FunctionName}",
                    NguoiThucHien = User.Identity?.Name,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    ThoiGian = DateTime.Now
                });
                await _context.SaveChangesAsync();
                return Ok(new { success = false, message = "[Aborted] Lệnh đã bị hủy an toàn." });
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

        // ── POST /api/AIChat/report ─────────────────────────────────────
        // Thu thập dữ liệu thực từ DB → gửi cho Gemini AI tự tạo HTML
        [Authorize(Roles = "SuperAdmin,ChuCuaHang,Admin")]
        [HttpPost("report")]
        public async Task<IActionResult> GenerateReport([FromBody] ReportRequest req)
        {
            var prompt = req.Prompt ?? "";
            var fn = req.FunctionName ?? "";
            var pLow = prompt.ToLower();
            var now = DateTime.Now;

            // ── 1. Xác định loại dữ liệu cần lấy ─────────────────────
            bool isStoreList = fn == "DanhSachCuaHang"
                || pLow.Contains("danh sách") || pLow.Contains("danh sach")
                || (pLow.Contains("cửa hàng") && !pLow.Contains("thống kê") && !pLow.Contains("tổng quan"));

            bool isNhatKy = fn == "XemNhatKy"
                || pLow.Contains("nhật ký") || pLow.Contains("nhat ky") || pLow.Contains("log");

            bool isSub = fn.Contains("DangKy") || fn.Contains("GiaHan")
                || pLow.Contains("đăng ký") || pLow.Contains("gia hạn") || pLow.Contains("thanh toán");

            bool isSapHetHan = pLow.Contains("sắp hết hạn") || pLow.Contains("hết hạn");

            // ── 2. Lấy dữ liệu thực từ DB ─────────────────────────────
            object realData;
            string context;

            if (isStoreList || isSapHetHan)
            {
                var q = _context.CuaHangs.AsQueryable();
                if (isSapHetHan && !isStoreList) q = q.Where(c => c.NgayHetHan <= now.AddDays(30));

                var stores = await q.OrderByDescending(c => c.NgayDangKy)
                    .Select(c => new {
                        c.Id, c.TenCuaHang, c.Email, c.TrangThai, c.GoiDichVu,
                        NgayDangKy = c.NgayDangKy.ToString("dd/MM/yyyy"),
                        NgayHetHan = c.NgayHetHan.ToString("dd/MM/yyyy"),
                        DaHetHan   = c.NgayHetHan < now,
                        SapHetHan7 = c.NgayHetHan >= now && c.NgayHetHan <= now.AddDays(7)
                    }).ToListAsync();

                context = $"Danh sách {stores.Count} cửa hàng";
                realData = new {
                    tongSo       = stores.Count,
                    hoatDong     = stores.Count(s => s.TrangThai == "HoatDong"),
                    dungThu      = stores.Count(s => s.TrangThai == "DungThu"),
                    biKhoa       = stores.Count(s => s.TrangThai == "BiKhoa"),
                    hetHan       = stores.Count(s => s.DaHetHan),
                    sapHetHan7   = stores.Count(s => s.SapHetHan7),
                    danhSach     = stores
                };
            }
            else if (isNhatKy)
            {
                var logs = await _context.NhatKyHeThangs
                    .OrderByDescending(n => n.ThoiGian).Take(100)
                    .Select(n => new {
                        n.HanhDong, n.MoTa, n.NguoiThucHien, n.IpAddress,
                        ThoiGian = n.ThoiGian.ToString("dd/MM/yyyy HH:mm")
                    }).ToListAsync();

                var theoLoai = logs.GroupBy(l => l.HanhDong)
                    .Select(g => new { HanhDong = g.Key, SoLan = g.Count() })
                    .OrderByDescending(x => x.SoLan).ToList();

                context = $"Nhật ký hệ thống ({logs.Count} bản ghi)";
                realData = new { tongBanGhi = logs.Count, theoLoai, chiTiet = logs };
            }
            else if (isSub)
            {
                var subs = await _context.LichSuDangKys
                    .Include(l => l.CuaHang).Include(l => l.GoiDichVu)
                    .OrderByDescending(l => l.NgayTao).Take(50).ToListAsync();

                var dtos = subs.Select(l => new {
                    TenCuaHang     = l.CuaHang?.TenCuaHang ?? "N/A",
                    TenGoi         = l.GoiDichVu?.TenGoi ?? $"Gói #{l.GoiDichVuId}",
                    l.SoTienThanhToan, l.TrangThai,
                    NgayBatDau     = l.NgayBatDau.ToString("dd/MM/yyyy"),
                    NgayKetThuc    = l.NgayKetThuc.ToString("dd/MM/yyyy"),
                    NgayTao        = l.NgayTao.ToString("dd/MM/yyyy HH:mm")
                }).ToList();

                var tongDT = subs.Where(s => s.TrangThai == "DaThanhToan").Sum(s => s.SoTienThanhToan);
                context = $"Lịch sử đăng ký/gia hạn ({subs.Count} giao dịch)";
                realData = new {
                    tongGiaoDich = subs.Count,
                    daThanhToan  = subs.Count(s => s.TrangThai == "DaThanhToan"),
                    choThanhToan = subs.Count(s => s.TrangThai == "ChoThanhToan"),
                    daHuy        = subs.Count(s => s.TrangThai == "DaHuy"),
                    tongDoanhThu = tongDT,
                    danhSach     = dtos
                };
            }
            else
            {
                // Mặc định: tổng quan SaaS
                var total   = await _context.CuaHangs.CountAsync();
                var active  = await _context.CuaHangs.CountAsync(c => c.TrangThai == "HoatDong");
                var trial   = await _context.CuaHangs.CountAsync(c => c.TrangThai == "DungThu");
                var locked  = await _context.CuaHangs.CountAsync(c => c.TrangThai == "BiKhoa");
                var chiDoc  = await _context.CuaHangs.CountAsync(c => c.TrangThai == "ChiDoc");
                var exp7    = await _context.CuaHangs.CountAsync(c => c.TrangThai == "HoatDong" && c.NgayHetHan <= now.AddDays(7));
                var exp30   = await _context.CuaHangs.CountAsync(c => c.NgayHetHan > now && c.NgayHetHan <= now.AddDays(30));
                var newMo   = await _context.CuaHangs.CountAsync(c => c.NgayDangKy >= new DateTime(now.Year, now.Month, 1));

                var revThis = await _context.LichSuDangKys
                    .Where(l => l.TrangThai == "DaThanhToan" && l.NgayThanhToan >= new DateTime(now.Year, now.Month, 1))
                    .SumAsync(l => (decimal?)l.SoTienThanhToan) ?? 0;
                var revLast = await _context.LichSuDangKys
                    .Where(l => l.TrangThai == "DaThanhToan"
                        && l.NgayThanhToan >= new DateTime(now.Year, now.Month, 1).AddMonths(-1)
                        && l.NgayThanhToan < new DateTime(now.Year, now.Month, 1))
                    .SumAsync(l => (decimal?)l.SoTienThanhToan) ?? 0;

                var topGoi = await _context.CuaHangs
                    .Where(c => c.GoiDichVu != null)
                    .GroupBy(c => c.GoiDichVu)
                    .Select(g => new { Goi = g.Key, SoCuaHang = g.Count() })
                    .OrderByDescending(x => x.SoCuaHang).Take(5).ToListAsync();

                context = "Tổng quan SaaS";
                realData = new {
                    thoiGian   = now.ToString("dd/MM/yyyy HH:mm"),
                    cuaHang    = new { total, active, trial, locked, chiDoc, exp7, exp30, newMo },
                    doanhThu   = new {
                        thangNay = revThis, thangTruoc = revLast,
                        tangTruong = revLast > 0 ? Math.Round((revThis - revLast) / revLast * 100, 1) : 0
                    },
                    topGoiDichVu = topGoi,
                    canhBao = new { exp7, exp30, locked, chiDoc }
                };
            }

            // ── 3. Gemini AI tự viết HTML từ dữ liệu thực ─────────────
            bool isSuperAdmin = User.IsInRole("SuperAdmin");
            var htmlReport = await _gemini.GenerateReportWithAI(prompt, realData, req.ModelId, isSuperAdmin);

            return Ok(new { htmlReport, prompt, generatedAt = now, context });
        }

        // ── DELETE /api/AIChat/session/{id} ────────────────────────────
        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("session/{sessionId}")]
        public IActionResult ClearSession(string sessionId)
        {
            lock (_lock) { _sessions.Remove(sessionId); }
            return Ok(new { message = "Session đã được xóa." });
        }

        // ── GET /api/AIChat/tools ───────────────────────────────────────
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("tools")]
        public IActionResult GetTools() =>
            Ok(GeminiAIService.AvailableTools.Select(t => new { t.Name, t.Description, t.RiskLevel }));

        // ── Tool execution ─────────────────────────────────────────────
        private async Task<ToolResult> ExecuteToolAsync(string fn, string argsJson)
        {
            try
            {
                var args = JsonDocument.Parse(argsJson).RootElement;
                return fn switch
                {
                    "ThongKeSaaS"      => await ToolThongKe(),
                    "DanhSachCuaHang"  => await ToolDanhSach(
                        args.TryGetProperty("trangThai", out var tt) ? tt.GetString() : null,
                        args.TryGetProperty("sapHetHan", out var sh) && sh.GetBoolean()),
                    "KhoaCuaHang"      => await ToolKhoa(
                        args.GetProperty("cuaHangId").GetInt32(),
                        args.TryGetProperty("lyDo", out var ld) ? ld.GetString() ?? "AI Agent" : "AI Agent"),
                    "MoKhoaCuaHang"    => await ToolMoKhoa(args.GetProperty("cuaHangId").GetInt32()),
                    "GiaHanGoi"        => await ToolGiaHan(
                        args.GetProperty("cuaHangId").GetInt32(),
                        args.TryGetProperty("soThang", out var sm) ? sm.GetInt32() : 1,
                        args.TryGetProperty("goiMoi", out var gm) ? gm.GetString() : null),
                    "GuiThongBao"      => await ToolThongBao(
                        args.TryGetProperty("cuaHangId", out var cid) ? cid.GetInt32() : 0,
                        args.GetProperty("tieuDe").GetString() ?? "",
                        args.GetProperty("noiDung").GetString() ?? "",
                        args.TryGetProperty("loai", out var lo) ? lo.GetString() ?? "ThongTin" : "ThongTin"),
                    "XemNhatKy"        => await ToolNhatKy(
                        args.TryGetProperty("tuNgay", out var tn) ? tn.GetString() : null,
                        args.TryGetProperty("hanhDong", out var hd) ? hd.GetString() : null),
                    "ThemGoiSaaS"      => await ToolThemGoiSaaS(
                        args.TryGetProperty("tenGoi",         out var tg)  ? tg.GetString()   : null,
                        args.TryGetProperty("maGoi",          out var mg)  ? mg.GetString()   : null,
                        args.TryGetProperty("soThang",        out var st)  ? st.GetInt32()    : 0,
                        args.TryGetProperty("giaThang",       out var gt)  ? gt.GetDecimal()  : 0,
                        args.TryGetProperty("tongGia",        out var tgi) ? tgi.GetDecimal() : 0,
                        args.TryGetProperty("gioiHanHoaDon",  out var ghd) ? ghd.GetInt32()   : 0,
                        args.TryGetProperty("gioiHanNhanVien",out var gnv) ? gnv.GetInt32()   : 0,
                        args.TryGetProperty("moTa",           out var mta) ? mta.GetString()  : null),
                    "ThietLapHeThong"  => await ToolThietLap(
                        args.TryGetProperty("key",   out var k) ? k.GetString() : null,
                        args.TryGetProperty("value", out var v) ? v.GetString() : null),
                    "XuatBaoCaoAI"     => new ToolResult { Success = true, Message = "Báo cáo AI đã được mở." },
                    _                  => new ToolResult { Success = false, Message = $"Tool '{fn}' không được hỗ trợ." }
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ExecuteTool error: {Fn}", fn);
                return new ToolResult { Success = false, Message = $"Lỗi thực thi: {ex.Message}" };
            }
        }

        // ── Tool implementations ────────────────────────────────────────
        private async Task<ToolResult> ToolThongKe()
        {
            var now = DateTime.Now;
            return new ToolResult { Success = true, Message = "Thống kê SaaS.", Data = new {
                tongCuaHang    = await _context.CuaHangs.CountAsync(),
                hoatDong       = await _context.CuaHangs.CountAsync(c => c.TrangThai == "HoatDong"),
                dungThu        = await _context.CuaHangs.CountAsync(c => c.TrangThai == "DungThu"),
                chiDoc         = await _context.CuaHangs.CountAsync(c => c.TrangThai == "ChiDoc"),
                biKhoa         = await _context.CuaHangs.CountAsync(c => c.TrangThai == "BiKhoa"),
                sapHetHan7Ngay = await _context.CuaHangs.CountAsync(c => c.TrangThai == "HoatDong" && c.NgayHetHan <= now.AddDays(7)),
                doanhThuThang  = await _context.LichSuDangKys
                    .Where(l => l.TrangThai == "DaThanhToan" && l.NgayThanhToan >= new DateTime(now.Year, now.Month, 1))
                    .SumAsync(l => (decimal?)l.SoTienThanhToan) ?? 0
            }};
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
            return new ToolResult { Success = true, Message = $"Đã mở khóa: [{ch.TenCuaHang}] → {ch.TrangThai}" };
        }

        private async Task<ToolResult> ToolGiaHan(int id, int soThang, string? goiMoi)
        {
            var ch = await _context.CuaHangs.FindAsync(id);
            if (ch == null) return new ToolResult { Success = false, Message = $"Không tìm thấy cửa hàng #{id}" };
            ch.NgayHetHan = (ch.NgayHetHan > DateTime.Now ? ch.NgayHetHan : DateTime.Now).AddMonths(soThang);
            ch.TrangThai = "HoatDong";
            if (!string.IsNullOrEmpty(goiMoi)) ch.GoiDichVu = goiMoi;
            await _context.SaveChangesAsync();
            return new ToolResult { Success = true, Message = $"Đã gia hạn [{ch.TenCuaHang}] +{soThang} tháng → {ch.NgayHetHan:dd/MM/yyyy}" };
        }

        private async Task<ToolResult> ToolThongBao(int cuaHangId, string tieuDe, string noiDung, string loai)
        {
            _context.ThongBaoHeThongs.Add(new ThongBaoHeThong {
                TieuDe = tieuDe, NoiDung = noiDung, LoaiThongBao = loai,
                CuaHangId = cuaHangId == 0 ? null : cuaHangId,
                NgayTao = DateTime.Now, DaDoc = false
            });
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
            return new ToolResult { Success = true, Message = $"{logs.Count} bản ghi.", Data = logs };
        }

        private async Task<ToolResult> ToolThemGoiSaaS(string? tenGoi, string? maGoi, int soThang,
            decimal giaThang, decimal tongGia, int gioiHanHoaDon, int gioiHanNhanVien, string? moTa)
        {
            if (string.IsNullOrEmpty(tenGoi) || string.IsNullOrEmpty(maGoi) || soThang <= 0)
                return new ToolResult { Success = false, Message = "Tên gói, mã gói và số tháng không hợp lệ." };
            if (await _context.GoiDichVus.AnyAsync(g => g.MaGoi == maGoi))
                return new ToolResult { Success = false, Message = $"Gói '{maGoi}' đã tồn tại." };
            var p = new GoiDichVu {
                TenGoi = tenGoi, MaGoi = maGoi, SoThang = soThang,
                GiaThang = giaThang, TongGia = tongGia > 0 ? tongGia : giaThang * soThang,
                GioiHanHoaDon = gioiHanHoaDon, GioiHanNhanVien = gioiHanNhanVien,
                MoTa = moTa, IsActive = true,
                ThuTuHienThi = await _context.GoiDichVus.CountAsync() + 1
            };
            _context.GoiDichVus.Add(p);
            await _context.SaveChangesAsync();
            return new ToolResult { Success = true, Message = $"Đã tạo gói: [{p.TenGoi}] ({p.MaGoi}) - {p.SoThang} tháng." };
        }

        private async Task<ToolResult> ToolThietLap(string? key, string? value)
        {
            if (string.IsNullOrEmpty(key) || value == null)
                return new ToolResult { Success = false, Message = "Thiếu Key hoặc Value." };
            var cfg = await _context.CauHinhHeThangs.FirstOrDefaultAsync(c => c.MaKey == key);
            if (cfg != null) { cfg.GiaTri = value; cfg.NgayCapNhat = DateTime.Now; cfg.NguoiCapNhat = User.Identity?.Name ?? "AI"; }
            else _context.CauHinhHeThangs.Add(new CauHinhHeThong {
                MaKey = key, GiaTri = value, NhomCauHinh = "System",
                NguoiCapNhat = User.Identity?.Name ?? "AI", NgayCapNhat = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return new ToolResult { Success = true, Message = $"Đã cập nhật: {key} = {value}" };
        }

        private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max] + "...";

        // ── POST /api/AIChat/ask ─────────────────────────────────────────
        // Dành cho Quản lý / Chủ cửa hàng / Nhân viên (Copilot Chatbot)
        [HttpPost("ask")]
        [Authorize]
        public async Task<IActionResult> Ask([FromBody] AskRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Question))
                return BadRequest(new { error = "Câu hỏi không được để trống!" });

            // SessionId phân biệt theo từng tài khoản
            var sessionId = User.Identity?.Name ?? "guest";
            List<GeminiMessage>? history;
            lock (_lock) { _sessions.TryGetValue(sessionId, out history); }

            // Xác định vai trò của người dùng để chọn Prompt phù hợp
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? req.Role ?? "NhanVien";
            string promptFileName = userRole switch
            {
                "ChuCuaHang" or "Admin" or "Owner" => "Chat_QuanLy.md",
                "ThuNgan" => "Chat_ThuNgan.md",
                "NhanVien" or "Order" or "Waiter" => "Chat_Order.md",
                _ => "Chat_Order.md"
            };

            // Gọi service với Owner API Key và prompt động theo vai trò
            var response = await _gemini.AskAsync(req.Question, promptFileName, history, req.ModelId);

            if (response.Error != null)
                return StatusCode(500, new { error = response.Error });

            // Lưu lịch sử hội thoại
            lock (_lock)
            {
                if (!_sessions.ContainsKey(sessionId))
                    _sessions[sessionId] = new List<GeminiMessage>();
                _sessions[sessionId].Add(new GeminiMessage { Role = "user", Text = req.Question });
                _sessions[sessionId].Add(new GeminiMessage
                {
                    Role = "model",
                    Text = response.Text
                });
                if (_sessions[sessionId].Count > 40) _sessions[sessionId].RemoveRange(0, 2);
            }

            // Ghi nhận nhật ký hoạt động
            _context.NhatKyHeThangs.Add(new NhatKyHeThong
            {
                HanhDong = "AICopilotAsk",
                MoTa = $"Chủ quán/Nhân viên hỏi AI: \"{Truncate(req.Question, 100)}\"",
                NguoiThucHien = User.Identity?.Name,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                ThoiGian = DateTime.Now
            });
            await _context.SaveChangesAsync();

            return Ok(new { answer = response.Text });
        }
    }

    // ── DTOs ──────────────────────────────────────────────────────────
    public class ChatRequest
    {
        public string Prompt { get; set; } = "";
        public string? SessionId { get; set; }
        public string? ModelId { get; set; }
        public string Mode { get; set; } = "agent";
    }
    public class ReportRequest
    {
        public string? Prompt { get; set; }
        public string? FunctionName { get; set; }
        public string? ModelId { get; set; }
    }
    public class ConfirmRequest
    {
        public bool Confirmed { get; set; }
        public string FunctionName { get; set; } = "";
        public string? FunctionArgs { get; set; }
    }
    public class ToolResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public object? Data { get; set; }
    }
    public class AskRequest
    {
        public string Question { get; set; } = "";
        public string? Role { get; set; }
        public string? ModelId { get; set; }
    }
}