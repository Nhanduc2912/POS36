# Báo cáo Tổng quan Dự án POS36

## 1. Mô tả chung

Dự án **POS36** là một hệ thống quản lý bán hàng (Point of Sale) được xây dựng với kiến trúc client-server hiện đại.

-   **Backend (`POS36.Api`):** Được phát triển bằng ASP.NET Core, cung cấp các API để xử lý logic nghiệp vụ, quản lý dữ liệu và giao tiếp với cơ sở dữ liệu thông qua Entity Framework Core.
-   **Frontend (`POS36.Web`):** Được phát triển bằng Vue.js, cung cấp giao diện người dùng (UI) cho nhân viên và quản lý để tương tác với hệ thống.
-   **Giao tiếp real-time:** Hệ thống sử dụng SignalR (`KitchenHub.cs`) cho các tính năng thời gian thực, ví dụ như gửi đơn hàng xuống bếp.
-   **Cơ sở dữ liệu:** Sử dụng SQL Server, với cấu trúc được quản lý bằng EF Core Migrations.

Đây là một hệ thống POS khá toàn diện, hướng tới đối tượng nhà hàng, quán cà phê.

## 2. Các chức năng chính (Dường như đã hoàn thiện hoặc đang phát triển)

Dựa trên cấu trúc controller ở backend và các view ở frontend, hệ thống có các chức năng cốt lõi sau:

-   **Quản lý Xác thực & Nhân viên (`AuthController`, `NhanVienController`, `Login.vue`, `Register.vue`, `EmployeeSetup.vue`):**
    -   Đăng nhập, đăng ký tài khoản.
    -   Quản lý thông tin nhân viên.

-   **Hệ thống POS & Quản lý Đơn hàng (`HoaDonController`, `PosView.vue`, `OrderView.vue`):**
    -   Giao diện bán hàng chính để tạo đơn hàng.
    -   Xem và quản lý danh sách các đơn hàng.

-   **Quản lý Sản phẩm & Menu (`SanPhamController`, `MenuController`, `DanhMucController`, `ProductSetup.vue`, `PriceSetup.vue`):**
    -   Thêm, sửa, xóa sản phẩm, danh mục.
    -   Thiết lập giá bán.

-   **Quản lý Bàn & Khu vực (`BanController`, `KhuVucController`, `TableSetup.vue`):**
    -   Thiết lập sơ đồ bàn, khu vực trong nhà hàng.
    -   Quản lý trạng thái bàn (trống, đang phục vụ...).

-   **Quản lý Kho (`KhoController`, `KiemKeController`, `ImportStock.vue`, `InventoryCheck.vue`):**
    -   Quản lý nhập kho, tồn kho.
    -   Thực hiện kiểm kê kho định kỳ.

-   **Báo cáo & Dashboard (`DashboardController`, `BaoCaoController`, `DashboardOverview.vue`):**
    -   Cung cấp cái nhìn tổng quan về hoạt động kinh doanh.
    -   Hệ thống báo cáo doanh thu, chi phí (cần kiểm tra mức độ chi tiết).

-   **Giao diện Bếp Thời gian thực (`KitchenHub.cs`, `KitchenView.vue`):**
    -   Gửi thông tin đơn hàng mới tới màn hình bếp ngay lập tức.

-   **Thanh toán (`ThanhToanController`):**
    -   Xử lý logic liên quan đến việc thanh toán hóa đơn.

## 3. Các chức năng cần xác minh hoặc có thể chưa hoàn thiện

Dựa vào cấu trúc file, một số chức năng sau có thể đã được lên kế hoạch nhưng chưa được triển khai đầy đủ trên giao diện người dùng, hoặc cần được kiểm tra lại:

-   **Quản lý Chi nhánh (`ChiNhanhController.cs`):**
    -   **Hiện trạng:** Đã có controller ở backend.
    -   **Vấn đề:** Không có view tương ứng trong thư mục `POS36.Web/src/views`. Điều này cho thấy chức năng quản lý nhiều chi nhánh có thể mới chỉ được xây dựng ở phía API và chưa có giao diện cho người dùng cuối.

-   **Quản lý Khách hàng:**
    -   **Hiện trạng:** Có model `KhachHang.cs` trong `POS36.Api/Models`.
    -   **Vấn đề:** Không có `KhachHangController` hay view nào liên quan. Tính năng quản lý thông tin khách hàng, chương trình khách hàng thân thiết... dường như đã được định hình trong cơ sở dữ liệu nhưng chưa được phát triển thành một chức năng hoàn chỉnh.

-   **Quản lý Nhà cung cấp & Nhập hàng:**
    -   **Hiện trạng:** Có các model `PhieuNhap.cs` và `ChiTietPhieuNhap.cs`.
    -   **Vấn đề:** Tương tự quản lý khách hàng, chưa thấy có controller hay view riêng cho việc quản lý nhà cung cấp và tạo phiếu nhập hàng từ nhà cung cấp một cách chi tiết. Chức năng `ImportStock.vue` có thể chỉ là một bước nhập kho đơn giản.

-   **Báo cáo Chi tiết (`BaoCaoController.cs`):**
    -   **Hiện trạng:** Đã có controller.
    -   **Vấn đề:** Chức năng báo cáo thường rất phức tạp. Cần phải kiểm tra xem các loại báo cáo (doanh thu theo ngày/tháng, lợi nhuận, mặt hàng bán chạy nhất...) đã được hiện thực hóa đầy đủ trên giao diện hay chưa.

-   **Tích hợp Cổng thanh toán:**
    -   **Hiện trạng:** `ThanhToanController.cs` đã tồn tại.
    -   **Vấn đề:** Controller này có thể chỉ đang ghi nhận các thanh toán (tiền mặt, chuyển khoản) một cách thủ công. Việc tích hợp với các cổng thanh toán của bên thứ ba (VNPAY, MoMo, ZaloPay...) là một hạng mục lớn và có thể chưa được thực hiện.

-   **Phân quyền Chi tiết:**
    -   **Hiện trạng:** Có hệ thống đăng nhập.
    -   **Vấn đề:** Chưa rõ hệ thống có hỗ trợ phân quyền chi tiết hay không (ví dụ: vai trò `Nhân viên` chỉ được bán hàng, vai trò `Quản lý` được xem báo cáo và cài đặt). Đây là một phần quan trọng nhưng thường được bổ sung ở các giai đoạn sau của dự án.
