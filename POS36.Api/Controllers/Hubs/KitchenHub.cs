using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using System;
using System.Threading.Tasks;

namespace POS36.Api.Hubs
{
    public class KitchenHub : Hub
    {
        private readonly AppDbContext _context;

        public KitchenHub(AppDbContext context)
        {
            _context = context;
        }

        // ===================================================================
        // QUẢN LÝ GROUPS: Mỗi cửa hàng = 1 Group riêng biệt
        // Tránh cross-tenant broadcast (cửa hàng A nhận event của cửa hàng B)
        // ===================================================================
        public override async Task OnConnectedAsync()
        {
            // Đọc CuaHangId từ JWT claims (đã được xác thực qua middleware)
            var cuaHangIdClaim = Context.User?.FindFirst("CuaHangId")?.Value;
            if (!string.IsNullOrEmpty(cuaHangIdClaim))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"store_{cuaHangIdClaim}");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var cuaHangIdClaim = Context.User?.FindFirst("CuaHangId")?.Value;
            if (!string.IsNullOrEmpty(cuaHangIdClaim))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"store_{cuaHangIdClaim}");
            }
            await base.OnDisconnectedAsync(exception);
        }

        // ===================================================================
        // 1. Order gọi → Phát cho Thu Ngân (cùng cửa hàng)
        // ===================================================================
        public async Task YeuCauThanhToan(int chiNhanhId, string tenBan)
        {
            var cuaHangId = Context.User?.FindFirst("CuaHangId")?.Value ?? "0";
            await Clients.Group($"store_{cuaHangId}")
                .SendAsync("CoYeuCauThanhToan", chiNhanhId, tenBan);

            // Ghi nhật ký báo thu ngân
            await _context.LogHoatDongAsync(chiNhanhId, "Báo thu ngân", $"Yêu cầu thu ngân tính tiền cho {tenBan}");
        }

        // ===================================================================
        // 2. Order/Thu ngân gọi → Phát cho Bếp (cùng cửa hàng)
        // ===================================================================
        public async Task SendOrderToKitchen(int chiNhanhId, string tenBan, string monAn)
        {
            var cuaHangId = Context.User?.FindFirst("CuaHangId")?.Value ?? "0";
            await Clients.Group($"store_{cuaHangId}")
                .SendAsync("ReceiveNewOrder", chiNhanhId, tenBan, monAn);
        }

        // ===================================================================
        // 3. Thu ngân gửi yêu cầu mở QR sang máy Nhân viên (cùng cửa hàng)
        // ===================================================================
        public async Task YeuCauMoQR(int banId, decimal soTien, string maChungTu)
        {
            var cuaHangId = Context.User?.FindFirst("CuaHangId")?.Value ?? "0";
            await Clients.Group($"store_{cuaHangId}")
                .SendAsync("NhanYeuCauMoQR", banId, soTien, maChungTu);

            // Tìm chi nhánh của bàn
            var ban = await _context.Bans.Include(b => b.KhuVuc).FirstOrDefaultAsync(b => b.Id == banId);
            int chiNhanhId = ban?.KhuVuc?.ChiNhanhId ?? 0;
            string tenBan = ban?.TenBan ?? $"Bàn {banId}";
            await _context.LogHoatDongAsync(chiNhanhId, "Tạo mã QR", $"Mở cổng thanh toán chuyển khoản QR cho {tenBan}. Số tiền: {soTien:N0}đ");
        }

        // ===================================================================
        // 4. Nhân viên báo lỗi/hủy QR (cùng cửa hàng)
        // ===================================================================
        public async Task HuyMoQR(int banId, string lyDo)
        {
            var cuaHangId = Context.User?.FindFirst("CuaHangId")?.Value ?? "0";
            await Clients.Group($"store_{cuaHangId}")
                .SendAsync("NhanHuyMoQR", banId, lyDo);

            var ban = await _context.Bans.Include(b => b.KhuVuc).FirstOrDefaultAsync(b => b.Id == banId);
            int chiNhanhId = ban?.KhuVuc?.ChiNhanhId ?? 0;
            string tenBan = ban?.TenBan ?? $"Bàn {banId}";
            await _context.LogHoatDongAsync(chiNhanhId, "Hủy QR", $"Hủy hiển thị mã QR thanh toán của {tenBan}. Lý do: {lyDo}");
        }

        // ===================================================================
        // 5. Webhook Ngân hàng báo tin: Tiền đã vào tài khoản! (cùng cửa hàng)
        // ===================================================================
        public async Task XacNhanTienVe(int banId)
        {
            var cuaHangId = Context.User?.FindFirst("CuaHangId")?.Value ?? "0";
            await Clients.Group($"store_{cuaHangId}")
                .SendAsync("ThanhToanQRThanhCong", banId);
        }
    }
}