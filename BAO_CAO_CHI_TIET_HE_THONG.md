# Báo Cáo Phân Tích Codebase & Luồng Hoạt Động Hệ Thống POS36

## 1. Tổng Quan Hệ Thống
Hệ thống POS36 là một giải pháp quản lý bán hàng (Point of Sale) toàn diện dành cho cửa hàng/nhà hàng, bao gồm 2 thành phần chính:
- **Backend (POS36.Api):** Xây dựng trên nền tảng .NET 9.0 (C#), cung cấp các RESTful APIs, sử dụng Entity Framework Core để tương tác cơ sở dữ liệu và SignalR để cập nhật dữ liệu thời gian thực (ví dụ: màn hình Bếp).
- **Frontend (POS36.Web):** Xây dựng bằng Vue.js (Vite), sử dụng Vue Router để quản lý điều hướng và phân quyền giao diện (Role-based access control).

## 2. Kiến Trúc Backend (POS36.Api)
Backend được tổ chức theo cấu trúc MVC cơ bản kết hợp Data Transfer Objects (DTOs):
- **Controllers:** Chứa các API endpoints xử lý nghiệp vụ chính:
  - `AuthController`: Đăng nhập, xác thực và cấp phát JWT token.
  - `BanController`, `KhuVucController`: Quản lý sơ đồ bàn và khu vực của quán.
  - `DanhMucController`, `SanPhamController`, `MenuController`: Quản lý danh mục, sản phẩm và thực đơn.
  - `HoaDonController`, `ThanhToanController`: Xử lý tạo hóa đơn, tính tiền, và thanh toán.
  - `KhoController`, `NhapHangController`, `KiemKeController`: Quản lý tồn kho, nhập hàng và kiểm kê.
  - `NhanVienController`: Quản lý tài khoản và phân quyền nhân viên.
  - `BaoCaoController`, `DashboardController`: Thống kê doanh thu, số liệu tổng quan.
  - `ThuChiController`: Quản lý sổ quỹ, phiếu thu/chi.
  - `ThietLapController`, `ChiNhanhController`: Các cài đặt hệ thống và cấu hình chi nhánh.
- **Hubs (`KitchenHub.cs`):** Sử dụng SignalR để đẩy thông báo thời gian thực từ bộ phận Order (Ghi nhận món) tới màn hình hiển thị của bộ phận Bếp.
- **Models & Data:** `AppDbContext.cs` định nghĩa các DbSets ứng với các bảng: `Ban`, `ChiNhanh`, `ChiTietHoaDon`, `HoaDon`, `KhachHang`, `SanPham`, `PhieuNhap`, `PhieuKiemKe`, `PhieuThuChi`, v.v.
- **Migrations:** Quản lý thay đổi cấu trúc database bằng EF Core Migrations.

## 3. Kiến Trúc Frontend (POS36.Web) & Luồng Phân Quyền
Frontend sử dụng Vue Router với chức năng Navigation Guards để chặn/điều hướng người dùng dựa vào chức năng và phân quyền (Roles).

### Cơ chế phân quyền (Roles):
- **Admin / ChuCuaHang / QuanLy:** Có toàn quyền truy cập vào giao diện Quản trị (`/admin/*`).
- **ThuNgan (Thu Ngân):** Chỉ được truy cập màn hình POS (`/pos`).
- **Order (Nhân viên phục vụ):** Chỉ được truy cập màn hình gọi món (`/order`).
- **Bep (Nhân viên bếp):** Chỉ được truy cập màn hình hiển thị bếp (`/kitchen`).

### Phân tích chi tiết các Trang (Pages/Views):
#### 3.1. Nhóm Trang Public & Xác thực
- **`LandingPage.vue` (`/`)**: Trang chủ giới thiệu phần mềm.
- **`Login.vue` (`/login`)**: Giao diện đăng nhập, gọi API AuthController để nhận JWT Token và Role.
- **`Register.vue` (`/register`)**: Đăng ký cửa hàng/tài khoản mới.

#### 3.2. Nhóm Trang Quản Trị (`/admin/*`) - Dành cho Quản lý
Sử dụng chung layout `AdminLayout.vue` chứa thanh menu điều hướng.
- **`DashboardOverview.vue` (`/admin`)**: Bảng điều khiển tổng quan, hiển thị thống kê, biểu đồ doanh thu, số liệu.
- **`TableSetup.vue` (`/admin/tables`)**: Thiết lập sơ đồ bàn, khu vực.
- **`ProductSetup.vue` (`/admin/products`)**: Quản lý danh sách sản phẩm, hình ảnh, thông tin cơ bản.
- **`PriceSetup.vue` (`/admin/prices`)**: Cấu hình giá bán cho từng sản phẩm.
- **`ImportStock.vue` & `CreateStock.vue` (`/admin/import-stock`, `/admin/import-create`)**: Quản lý phiếu nhập kho và chức năng nhập hàng mới.
- **`EmployeeSetup.vue` (`/admin/employees`)**: Quản lý tài khoản nhân viên, cấp quyền truy cập.
- **`OrderList.vue` (`/admin/orders`)**: Xem danh sách các hóa đơn, lịch sử giao dịch.
- **`InventoryCheck.vue` & `InventoryCreate.vue` (`/admin/inventory`)**: Quản lý và tạo các phiếu kiểm kê kho định kỳ.
- **`Cashbook.vue` (`/admin/cashbook`)**: Sổ quỹ, quản lý dòng tiền (Phiếu thu / Phiếu chi).

#### 3.3. Nhóm Trang Tác Nghiệp (Fullscreen)
Các màn hình này được thiết kế tối ưu hóa UX/UI cho thao tác nhanh của từng bộ phận:
- **`PosView.vue` (`/pos`)**: Màn hình thu ngân chuyên dụng. Quản lý trạng thái bàn, thêm món, áp dụng mã giảm giá và thanh toán.
- **`OrderView.vue` (`/order`)**: Màn hình dành cho nhân viên phục vụ cầm máy tính bảng/điện thoại để ghi nhận order tại bàn và gửi thông tin vào bếp.
- **`KitchenView.vue` (`/kitchen`)**: Màn hình hiển thị tại bếp. Nhận dữ liệu realtime qua SignalR (`signalr.js` + `KitchenHub`). Hiển thị danh sách món cần làm, trạng thái đang nấu/đã xong.

## 4. Luồng Hoạt Động (Workflow) Cốt Lõi Của Hệ Thống

### Luồng Bán Hàng & Phục Vụ (Order -> Kitchen -> POS)
1. **Ghi nhận món (Order):** Nhân viên phục vụ dùng `OrderView.vue` để chọn bàn và đặt món. Khi xác nhận, dữ liệu được gửi về `HoaDonController`.
2. **Thông báo Bếp (Kitchen):** API gọi `KitchenHub` để broadcast (SignalR) danh sách món mới đến `KitchenView.vue`.
3. **Chế biến (Kitchen):** Bếp tiếp nhận món, khi làm xong sẽ cập nhật trạng thái trên màn hình Bếp. Trạng thái này có thể báo ngược lại cho Phục vụ/Thu ngân.
4. **Thanh toán (POS):** Khách hàng ra quầy, Thu ngân dùng `PosView.vue` chọn bàn tương ứng. Giao diện lấy thông tin hóa đơn (gồm cả chi tiết các món). Thu ngân tính tiền, in hóa đơn (gọi `ThanhToanController`). Bàn được giải phóng, ghi nhận doanh thu vào báo cáo và cập nhật trừ tồn kho tự động.

### Luồng Quản Lý Kho (Nhập hàng & Kiểm kê)
1. **Nhập hàng mới:** Quản lý tạo phiếu nhập (`CreateStock.vue`) thông qua `NhapHangController`. Dữ liệu lưu vào bảng `PhieuNhap` và tự động cộng dồn số lượng vào `TonKho`.
2. **Kiểm kê:** Định kỳ, quản lý tạo phiếu kiểm kê (`InventoryCreate.vue`), ghi nhận số lượng thực tế tại kho so với phần mềm. Hệ thống sinh phiếu chênh lệch và điều chỉnh lại tồn kho hiện tại.

### Luồng Thu Chi & Báo Cáo
1. Các khoản thu từ bán hàng tự động ghi vào doanh thu. Các khoản chi trả nhà cung cấp khi nhập hàng hoặc chi phí khác được lập trong Sổ quỹ (`Cashbook.vue`).
2. Dữ liệu tổng hợp được đưa lên biểu đồ tại `DashboardOverview.vue` thông qua `DashboardController` và `BaoCaoController`.

## 5. Kết Luận
POS36 là một hệ thống có kiến trúc rõ ràng, phân tách rạch ròi giữa các nghiệp vụ:
- **Tính bảo mật:** Áp dụng chặt chẽ JWT Token và Route Navigation Guards trên Frontend.
- **Tính thời gian thực:** Áp dụng tốt SignalR cho bài toán màn hình bếp (Kitchen Display System - KDS), giúp tối ưu thời gian giao tiếp giữa phục vụ và nhà bếp.
- **Tính trọn vẹn nghiệp vụ:** Đáp ứng đầy đủ từ khâu Bán hàng (POS), Quản lý Kho, Sổ quỹ, Nhân viên cho đến Báo cáo thống kê. Codebase được chia component và API endpoints rất hợp lý, dễ dàng bảo trì và mở rộng về sau.
