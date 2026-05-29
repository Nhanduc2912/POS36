using Microsoft.AspNetCore.SignalR;

namespace POS36.Api.Hubs
{
    public class KitchenHub : Hub
    {
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
        }

        // ===================================================================
        // 4. Nhân viên báo lỗi/hủy QR (cùng cửa hàng)
        // ===================================================================
        public async Task HuyMoQR(int banId, string lyDo)
        {
            var cuaHangId = Context.User?.FindFirst("CuaHangId")?.Value ?? "0";
            await Clients.Group($"store_{cuaHangId}")
                .SendAsync("NhanHuyMoQR", banId, lyDo);
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