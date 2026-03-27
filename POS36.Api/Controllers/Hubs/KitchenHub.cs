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
        // 1. Thu ngân gửi yêu cầu mở QR sang máy Nhân viên
        public async Task YeuCauMoQR(int banId, decimal soTien, string maChungTu)
        {
            await Clients.All.SendAsync("NhanYeuCauMoQR", banId, soTien, maChungTu);
        }

        // 2. Nhân viên báo lỗi/hủy QR (Khách đổi ý, sai bill...)
        public async Task HuyMoQR(int banId, string lyDo)
        {
            await Clients.All.SendAsync("NhanHuyMoQR", banId, lyDo);
        }

        // 3. Webhook Ngân hàng báo tin: Tiền đã vào tài khoản!
        public async Task XacNhanTienVe(int banId)
        {
            await Clients.All.SendAsync("ThanhToanQRThanhCong", banId);
        }
    }
}