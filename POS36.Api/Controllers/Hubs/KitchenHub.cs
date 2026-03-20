using Microsoft.AspNetCore.SignalR;

namespace POS36.Api.Hubs
{
    public class KitchenHub : Hub
    {
        // Hàm này Order gọi -> Phát cho Thu Ngân
        public async Task YeuCauThanhToan(int chiNhanhId, string tenBan)
        {
            await Clients.All.SendAsync("CoYeuCauThanhToan", chiNhanhId, tenBan);
        }

        // Hàm này Order/Thu ngân gọi -> Phát cho Bếp
        public async Task SendOrderToKitchen(int chiNhanhId, string tenBan, string monAn)
        {
            await Clients.All.SendAsync("ReceiveNewOrder", chiNhanhId, tenBan, monAn);
        }
    }
}