using Microsoft.AspNetCore.SignalR;

namespace POS36.Api.Hubs
{
    // Kế thừa từ class Hub của thư viện SignalR
    public class KitchenHub : Hub
    {
        // Tạm thời mình để trống. 
        // Sau này nếu Bếp làm xong món và muốn bấm nút "Báo cho phục vụ ra lấy",
        // thì mình sẽ viết hàm ở trong này để Bếp gọi ngược lên Server.
    }
}