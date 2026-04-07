<div align="center">

```
██████╗  ██████╗  ███████╗ ██████╗  ██████╗
██╔══██╗██╔═══██╗██╔════╝ ╚════██╗██╔════╝
██████╔╝██║   ██║╚███████╗  ███╔╝ ███████╗
██╔═══╝ ██║   ██║ ╚════██║ ███╔╝  ██╔═══██╗
██║     ╚██████╔╝ ███████║██████╗ ╚██████╔╝
╚═╝      ╚═════╝  ╚══════╝╚═════╝  ╚═════╝
```

# 🍽️ POS36 — Hệ Thống Quản Lý Bán Hàng F&B

**Giải pháp Point of Sale toàn diện dành cho Nhà hàng · Quán Cà phê · Cửa hàng Ăn uống**

[![ASP.NET Core](https://img.shields.io/badge/Backend-.NET%209.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![Vue.js](https://img.shields.io/badge/Frontend-Vue.js%203-42b883?style=for-the-badge&logo=vuedotjs)](https://vuejs.org/)
[![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-CC2927?style=for-the-badge&logo=microsoftsqlserver)](https://www.microsoft.com/sql-server)
[![SignalR](https://img.shields.io/badge/Realtime-SignalR-ff6b35?style=for-the-badge)](https://dotnet.microsoft.com/apps/aspnet/signalr)

</div>

---

## 📋 Mục Lục

- [Giới thiệu](#-giới-thiệu)
- [Công Nghệ Sử Dụng](#-công-nghệ-sử-dụng)
- [Cấu Trúc Dự Án](#-cấu-trúc-dự-án)
- [Mô Hình Dữ Liệu](#-mô-hình-dữ-liệu)
- [Toàn Bộ API Endpoints](#-toàn-bộ-api-endpoints)
- [Hệ Thống Giao Diện (Views)](#-hệ-thống-giao-diện-views)
- [Phân Quyền & Luồng Điều Hướng](#-phân-quyền--luồng-điều-hướng)
- [Luồng Hoạt Động Chi Tiết](#-luồng-hoạt-động-chi-tiết)
- [Hệ thống Real-time (SignalR)](#-hệ-thống-real-time-signalr)
- [Cài Đặt & Khởi Chạy](#-cài-đặt--khởi-chạy)

---

## 🚀 Giới Thiệu

**POS36** là một hệ thống quản lý bán hàng (Point of Sale) thế hệ mới, được thiết kế chuyên biệt cho mô hình kinh doanh F&B (Food & Beverage). Điểm nổi bật của hệ thống là kiến trúc đa vai trò — mỗi bộ phận trong nhà hàng có một màn hình riêng, được tối ưu hóa cho đúng công việc của họ.

### Điểm Nổi Bật

| Tính Năng | Mô Tả |
|:---|:---|
| 🤖 **AI Copilot tích hợp** | Trợ lý ảo Google Gemini 2.5 Flash, hỗ trợ phân tích dữ liệu kinh doanh theo thời gian thực |
| 📡 **Real-time với SignalR** | Đồng bộ tức thời giữa Phục vụ → Bếp → Thu ngân, hỗ trợ thanh toán QR Bank real-time |
| 🏪 **Multi-branch** | Quản lý nhiều chi nhánh trong một tài khoản Chủ cửa hàng |
| 🔐 **RBAC bảo mật** | Phân quyền chi tiết theo vai trò: 5 loại tài khoản với màn hình riêng biệt |
| 📊 **Báo cáo thông minh** | Dashboard, báo cáo doanh thu, sổ quỹ, kiểm kê kho tự động |
| 🖨️ **In hóa đơn** | Cấu hình mẫu in linh hoạt, hỗ trợ QR Code thanh toán ngân hàng |

---

## 🛠️ Công Nghệ Sử Dụng

### Backend (`POS36.Api`)

| Package | Phiên Bản | Mục Đích |
|:---|:---|:---|
| **ASP.NET Core** | 9.0 | Framework Web API chính |
| **Entity Framework Core** | 9.0 | ORM, quản lý Database |
| **EF Core SQL Server** | 9.0 | Provider kết nối SQL Server |
| **Microsoft.AspNetCore.Authentication.JwtBearer** | 9.0 | Xác thực JWT Token |
| **Swashbuckle.AspNetCore** | 7.2.0 | Swagger UI tài liệu API |
| **BCrypt.Net-Next** | 4.0.3 | Mã hóa mật khẩu |
| **Serilog.AspNetCore** | 10.0.0 | Logging màu sắc cho Terminal |
| **SignalR** | (Built-in) | Giao tiếp real-time |

### Frontend (`POS36.Web`)

| Package | Phiên Bản | Mục Đích |
|:---|:---|:---|
| **Vue.js** | ^3.5.30 | Framework UI chính |
| **Vue Router** | ^5.0.3 | Điều hướng trang & Navigation Guards |
| **Axios** | ^1.13.6 | HTTP Client gọi API |
| **@microsoft/signalr** | ^10.0.0 | Kết nối SignalR real-time |
| **Bootstrap** | ^5.3.8 | CSS Framework UI |
| **Bootstrap Icons** | ^1.13.1 | Bộ icon |
| **Chart.js + vue-chartjs** | ^4.5.1 | Biểu đồ doanh thu |
| **SweetAlert2** | ^11.26.23 | Hộp thoại thông báo đẹp |
| **@emailjs/browser** | ^4.4.1 | Gửi email OTP quên mật khẩu |
| **xlsx** | ^0.18.5 | Xuất Excel báo cáo |
| **Vite** | ^8.0.0 | Build tool & Dev server |

---

## 📂 Cấu Trúc Dự Án

```
POS36/
│
├── 📄 README.md
├── 📄 BAO_CAO_DU_AN.md            # Báo cáo tổng quan dự án
├── 📄 BAO_CAO_CHI_TIET_HE_THONG.md # Phân tích luồng hệ thống chi tiết
├── 📄 Pos36DB.sql                  # Script SQL khởi tạo Database
├── 🔧 ngrok.exe                    # Tool tunnel cho dev/demo
│
├── 📁 POS36.Api/                   # ── BACKEND (ASP.NET Core 9.0) ──
│   ├── Controllers/
│   │   ├── AuthController.cs       # Đăng ký, đăng nhập, quên mật khẩu OTP
│   │   ├── HoaDonController.cs     # Gọi món, thanh toán, chuyển/ghép bàn
│   │   ├── SanPhamController.cs    # CRUD sản phẩm, upload ảnh
│   │   ├── DanhMucController.cs    # Quản lý danh mục sản phẩm
│   │   ├── MenuController.cs       # Quản lý thực đơn hiển thị
│   │   ├── BanController.cs        # Trạng thái bàn
│   │   ├── KhuVucController.cs     # Quản lý khu vực
│   │   ├── NhanVienController.cs   # Quản lý nhân viên & tài khoản
│   │   ├── KhoController.cs        # Thông tin tồn kho
│   │   ├── NhapHangController.cs   # Phiếu nhập hàng, cập nhật tồn kho
│   │   ├── KiemKeController.cs     # Phiếu kiểm kê, cân bằng kho
│   │   ├── ThuChiController.cs     # Sổ quỹ, phiếu thu/chi
│   │   ├── DashboardController.cs  # Thống kê tổng quan Dashboard
│   │   ├── BaoCaoController.cs     # Báo cáo doanh thu
│   │   ├── ReportController.cs     # Báo cáo bổ sung
│   │   ├── AIChatController.cs     # AI Copilot (Google Gemini 2.5 Flash)
│   │   ├── ThietLapController.cs   # Cấu hình: Chi nhánh, Khu vực, Bàn, Mẫu in
│   │   ├── ChiNhanhController.cs   # Quản lý chi nhánh
│   │   ├── CuaHangController.cs    # Thông tin cửa hàng
│   │   ├── ProfileController.cs    # Hồ sơ cá nhân người dùng
│   │   ├── ThanhToanController.cs  # Lịch sử thanh toán
│   │   ├── WebhookController.cs    # Webhook nhận thông báo chuyển khoản ngân hàng
│   │   └── Hubs/
│   │       └── KitchenHub.cs       # SignalR Hub trung tâm (Bếp, QR, Thu ngân)
│   │
│   ├── Models/                     # Entities / Database Schema
│   │   ├── CuaHang.cs              # Bảng cửa hàng
│   │   ├── ChiNhanh.cs             # Bảng chi nhánh
│   │   ├── TaiKhoan.cs             # Bảng tài khoản đăng nhập
│   │   ├── NhanVien.cs             # Bảng hồ sơ nhân viên
│   │   ├── KhachHang.cs            # Bảng khách hàng
│   │   ├── KhuVuc.cs               # Bảng khu vực
│   │   ├── Ban.cs                  # Bảng bàn
│   │   ├── DanhMuc.cs              # Bảng danh mục sản phẩm
│   │   ├── SanPham.cs              # Bảng sản phẩm
│   │   ├── HoaDon.cs               # Bảng hóa đơn
│   │   ├── ChiTietHoaDon.cs        # Bảng chi tiết món trong hóa đơn
│   │   ├── ThanhToan.cs            # Bảng thanh toán
│   │   ├── TonKho.cs               # Bảng tồn kho
│   │   ├── PhieuNhap.cs            # Bảng phiếu nhập hàng
│   │   ├── ChiTietPhieuNhap.cs     # Bảng chi tiết phiếu nhập
│   │   ├── LichSuKho.cs            # Bảng lịch sử biến động kho
│   │   ├── PhieuKiemKe.cs          # Bảng phiếu kiểm kê
│   │   ├── ChiTietKiemKe.cs        # Bảng chi tiết kiểm kê
│   │   ├── PhieuThuChi.cs          # Bảng phiếu thu/chi (Sổ quỹ)
│   │   └── ThietLap.cs             # Bảng lưu cấu hình (JSON dạng key-value)
│   │
│   ├── DTOs/                       # Data Transfer Objects
│   │   ├── AuthDto.cs              # Register, Login, ForgotPassword, ResetPassword
│   │   ├── HoaDonDto.cs            # TaoDonHang, ChuyenBan, GhepBan
│   │   ├── SanPhamDTO.cs           # Thêm/Sửa sản phẩm
│   │   ├── DanhMucDTO.cs           # Danh mục
│   │   ├── MenuDto.cs              # Menu
│   │   ├── NhanVienDto.cs          # Nhân viên
│   │   ├── KhoDto.cs               # Kho
│   │   ├── KiemKeDto.cs            # Kiểm kê
│   │   ├── ThanhToanDto.cs         # Thanh toán
│   │   ├── BaoCaoDto.cs            # Báo cáo
│   │   └── ThietLapDto.cs          # Thiết lập (Chi nhánh, Khu vực, Bàn, Cấu hình)
│   │
│   ├── Data/
│   │   └── AppDbContext.cs         # EF Core DbContext — 6 phân hệ, 20 bảng
│   │
│   ├── Prompts/                    # System Prompts cho AI Copilot
│   │   ├── Chat_Order.md           # Prompt cho nhân viên Order
│   │   ├── Chat_ThuNgan.md         # Prompt cho Thu ngân
│   │   ├── Chat_QuanLy.md          # Prompt cho Quản lý/Admin (kèm live data)
│   │   └── ReportCopilot.md        # Prompt sinh báo cáo HTML
│   │
│   ├── Migrations/                 # EF Core Database migrations
│   ├── Program.cs                  # Startup: DI, JWT, CORS, Serilog, Middleware
│   └── appsettings.json            # Cấu hình: ConnectionString, JWT Key, Gemini API Key
│
└── 📁 POS36.Web/                   # ── FRONTEND (Vue.js 3 + Vite) ──
    ├── src/
    │   ├── main.js                 # Entry point, khai báo App, Router, Bootstrap
    │   ├── App.vue                 # Root component
    │   ├── store.js                # Simple state (chiNhanhId đang chọn)
    │   ├── style.css               # Global styles
    │   │
    │   ├── router/
    │   │   └── index.js            # Tất cả routes + Navigation Guards phân quyền
    │   │
    │   ├── services/
    │   │   └── signalr.js          # Khởi tạo & quản lý kết nối SignalR Hub
    │   │
    │   ├── views/
    │   │   ├── Home/               # Nhóm trang công khai (Landing)
    │   │   │   ├── LandingPage.vue     # Trang chủ giới thiệu phần mềm
    │   │   │   ├── FeaturesView.vue    # Trang tính năng
    │   │   │   ├── PricingView.vue     # Trang giá cả
    │   │   │   ├── AboutView.vue       # Trang giới thiệu
    │   │   │   ├── SolutionsView.vue   # Trang giải pháp
    │   │   │   └── PrivacyView.vue     # Trang chính sách bảo mật
    │   │   │
    │   │   ├── Login.vue               # Đăng nhập
    │   │   ├── Register.vue            # Đăng ký cửa hàng mới
    │   │   ├── ForgotPasswordView.vue  # Quên mật khẩu (OTP qua Email)
    │   │   │
    │   │   ├── AdminLayout.vue         # Layout Quản trị (sidebar menu)
    │   │   ├── DashboardOverview.vue   # Dashboard tổng quan + Chart 7 ngày
    │   │   ├── OrderList.vue           # Danh sách hóa đơn, lọc/tìm kiếm
    │   │   ├── ProductSetup.vue        # Quản lý sản phẩm (CRUD + ảnh)
    │   │   ├── PriceSetup.vue          # Cấu hình giá bán
    │   │   ├── TableSetup.vue          # Thiết lập sơ đồ bàn
    │   │   ├── EmployeeSetup.vue       # Quản lý nhân viên & tài khoản
    │   │   ├── ImportStock.vue         # Danh sách phiếu nhập kho
    │   │   ├── CreateStock.vue         # Tạo phiếu nhập hàng mới
    │   │   ├── InventoryCheck.vue      # Danh sách phiếu kiểm kê
    │   │   ├── InventoryCreate.vue     # Tạo phiếu kiểm kê mới
    │   │   ├── Cashbook.vue            # Sổ quỹ (Thu/Chi)
    │   │   ├── PrintSetup.vue          # Cấu hình mẫu in hóa đơn
    │   │   ├── BankSetup.vue           # Cấu hình tài khoản ngân hàng (QR)
    │   │   ├── AiReportView.vue        # Báo cáo AI (Google Gemini)
    │   │   ├── DailySummary.vue        # Tổng kết ca / ngày
    │   │   ├── SalesReport.vue         # Báo cáo doanh thu
    │   │   ├── ProfileView.vue         # Hồ sơ cá nhân
    │   │   ├── StoreInfoView.vue       # Thông tin cửa hàng
    │   │   │
    │   │   ├── PosView.vue             # 💰 Màn hình Thu ngân (POS)
    │   │   ├── OrderView.vue           # 📝 Màn hình Phục vụ / Gọi món
    │   │   └── KitchenView.vue         # 🍳 Màn hình Bếp (KDS)
    │   │
    │   ├── components/             # Shared components
    │   └── utils/                  # Utilities
    │
    ├── package.json
    ├── vite.config.js
    └── index.html
```

---

## 🗄️ Mô Hình Dữ Liệu

Hệ thống gồm **20 bảng** chia làm **6 phân hệ**:

### Phân Hệ 1 — Hệ Thống & Nhân Sự
```
CuaHang ──< ChiNhanh ──< TaiKhoan >── NhanVien
                      └─ KhuVuc ──< Ban
```
| Bảng | Mô Tả |
|:---|:---|
| `CuaHang` | Thông tin cửa hàng (TenCuaHang, SoDienThoai, NgayDangKy) |
| `ChiNhanh` | Chi nhánh thuộc cửa hàng (TenChiNhanh, DiaChi) |
| `TaiKhoan` | Thông tin đăng nhập (TenDangNhap, MatKhauHash BCrypt, VaiTro) |
| `NhanVien` | Hồ sơ nhân viên (MaNhanVien, TenNhanVien, SoDienThoai, Email) |
| `KhachHang` | Thông tin khách hàng |

### Phân Hệ 2 — Bán Hàng (Menu & Bàn)
| Bảng | Mô Tả |
|:---|:---|
| `KhuVuc` | Khu vực trong chi nhánh (Tầng 1, Tầng 2, Sân vườn...) |
| `Ban` | Bàn trong khu vực (MaBan, TenBan, TrangThai: Trống/Đang phục vụ) |
| `DanhMuc` | Danh mục sản phẩm (Đồ uống, Món chính, Tráng miệng...) |
| `SanPham` | Sản phẩm (TenSanPham, GiaBan, AnhUrl, TrangThai) |

### Phân Hệ 3 — Giao Dịch
| Bảng | Mô Tả |
|:---|:---|
| `HoaDon` | Hóa đơn (BanId, TrangThai, TongTien, PhuongThucThanhToan, NgayThanhToan) |
| `ChiTietHoaDon` | Chi tiết món (SanPhamId, SoLuong, DonGia, TrangThaiMon, GhiChu) |
| `ThanhToan` | Lịch sử giao dịch thanh toán |

### Phân Hệ 4 — Quản Lý Kho
| Bảng | Mô Tả |
|:---|:---|
| `TonKho` | Số lượng tồn kho theo chi nhánh (SanPhamId, ChiNhanhId, SoLuong) |
| `PhieuNhap` | Phiếu nhập hàng (MaChungTu, NgayNhap, TrangThai) |
| `ChiTietPhieuNhap` | Chi tiết phiếu nhập (SanPhamId, SoLuong, DonGiaNhap) |
| `LichSuKho` | Log biến động tồn kho |

### Phân Hệ 5 — Kiểm Kê
| Bảng | Mô Tả |
|:---|:---|
| `PhieuKiemKe` | Phiếu kiểm kê (MaChungTu, TrangThai: Nháp/Hoàn thành) |
| `ChiTietKiemKe` | Chi tiết kiểm kê (TonKhoHienTai vs SoLuongKiemKe) |

### Phân Hệ 6 — Sổ Quỹ & Cấu Hình
| Bảng | Mô Tả |
|:---|:---|
| `PhieuThuChi` | Phiếu thu/chi (LoaiPhieu, PhuongThuc, GiaTri, HangMuc, LyDo) |
| `ThietLap` | Cấu hình dạng key-value JSON (MaThietLap, DuLieu) |

---

## 🔌 Toàn Bộ API Endpoints

> Base URL: `http://localhost:5198/api`  
> Tất cả endpoints trừ Auth đều yêu cầu Header: `Authorization: Bearer <JWT_TOKEN>`

### 🔐 Auth (`/api/Auth`)

| Method | Endpoint | Mô Tả | Auth |
|:---:|:---|:---|:---:|
| POST | `/auth/register` | Đăng ký cửa hàng mới (tạo CuaHang + ChiNhanh + NhanVien + TaiKhoan) | ❌ |
| POST | `/auth/login` | Đăng nhập, trả về JWT Token + VaiTro | ❌ |
| POST | `/auth/forgot-password` | Tra cứu email → trả về OTP code và tên đăng nhập | ❌ |
| POST | `/auth/reset-password` | Xác nhận OTP → đổi mật khẩu mới | ❌ |

### 📄 Hóa Đơn (`/api/HoaDon`)

| Method | Endpoint | Mô Tả |
|:---:|:---|:---|
| POST | `/hoadon/goimon` | Gọi món cho bàn (tạo/cập nhật HoaDon + broadcast SignalR xuống Bếp) |
| GET | `/hoadon/ban/{banId}` | Lấy chi tiết hóa đơn đang mở của 1 bàn |
| POST | `/hoadon/chuyenban` | Chuyển toàn bộ hóa đơn sang bàn khác |
| POST | `/hoadon/ghepban` | Ghép 2 hóa đơn của 2 bàn vào 1 |
| POST | `/hoadon/thanhtoan/{banId}` | Thanh toán: đổi trạng thái, trừ tồn kho, tạo Phiếu Thu, broadcast SignalR |
| GET | `/hoadon/bep/danh-sach` | Lấy danh sách món "Chờ chế biến" cho màn hình Bếp |
| PUT | `/hoadon/bep/xong/{chiTietId}` | Bếp đánh dấu món đã làm xong → broadcast SignalR |
| GET | `/hoadon/danh-sach-admin` | Lấy danh sách hóa đơn cho Admin (lọc theo ngày, trạng thái, tìm kiếm) |
| POST | `/hoadon/huymon` | Hủy món / trả đồ (giảm số lượng hoặc xóa khỏi hóa đơn) |

### 🛍️ Sản Phẩm & Menu (`/api/SanPham`, `/api/DanhMuc`, `/api/Menu`)

| Method | Endpoint | Mô Tả |
|:---:|:---|:---|
| GET | `/sanpham` | Lấy danh sách sản phẩm |
| POST | `/sanpham` | Thêm sản phẩm mới (hỗ trợ upload ảnh) |
| PUT | `/sanpham/{id}` | Cập nhật sản phẩm |
| DELETE | `/sanpham/{id}` | Xóa/ẩn sản phẩm |
| GET | `/danhmuc` | Lấy danh sách danh mục |
| POST | `/danhmuc` | Thêm danh mục |
| PUT | `/danhmuc/{id}` | Sửa danh mục |
| DELETE | `/danhmuc/{id}` | Xóa danh mục |
| GET | `/menu` | Lấy menu (sản phẩm đang kích hoạt theo danh mục) |

### 🏠 Thiết Lập Sơ Đồ (`/api/ThietLap`)

| Method | Endpoint | Mô Tả |
|:---:|:---|:---|
| POST | `/thietlap/chinhanh` | Thêm chi nhánh mới |
| GET | `/thietlap/chinhanh` | Lấy danh sách chi nhánh |
| POST | `/thietlap/khuvuc` | Thêm khu vực vào chi nhánh |
| POST | `/thietlap/ban` | Thêm bàn vào khu vực |
| GET | `/thietlap/sodoban/{chiNhanhId}` | Lấy sơ đồ bàn đầy đủ (KhuVuc → Bàn) |
| GET | `/thietlap/{maThietLap}` | Lấy cấu hình (BankConfig, PrintConfig...) |
| POST | `/thietlap` | Lưu cấu hình (Upsert) |

### 👥 Nhân Viên (`/api/NhanVien`)

| Method | Endpoint | Mô Tả |
|:---:|:---|:---|
| GET | `/nhanvien` | Lấy danh sách nhân viên kèm tài khoản |
| POST | `/nhanvien` | Thêm nhân viên + tạo tài khoản với vai trò chỉ định |
| PUT | `/nhanvien/{id}` | Cập nhật thông tin nhân viên |
| DELETE | `/nhanvien/{id}` | Xóa nhân viên |

### 📦 Kho Hàng (`/api/Kho`, `/api/NhapHang`, `/api/KiemKe`)

| Method | Endpoint | Mô Tả |
|:---:|:---|:---|
| GET | `/kho` | Xem tồn kho hiện tại theo chi nhánh |
| GET | `/nhaphang/danh-sach` | Danh sách phiếu nhập (lọc ngày, trạng thái) |
| POST | `/nhaphang` | Tạo phiếu nhập hàng (cộng tồn kho nếu "Hoàn thành", tạo phiếu chi) |
| GET | `/kiemke/danh-sach` | Danh sách phiếu kiểm kê |
| GET | `/kiemke/san-pham-ton-kho` | Lấy toàn bộ sản phẩm kèm số lượng tồn để điền phiếu kiểm kê |
| POST | `/kiemke` | Tạo phiếu kiểm kê (ghi đè tồn kho nếu "Hoàn thành") |
| GET | `/kiemke/{id}` | Xem chi tiết 1 phiếu kiểm kê |

### 💰 Sổ Quỹ & Báo Cáo

| Method | Endpoint | Mô Tả |
|:---:|:---|:---|
| GET | `/thuchi/danh-sach` | Danh sách phiếu thu/chi + thống kê Tổng thu, Tổng chi, Tồn quỹ |
| GET | `/dashboard/summary` | Thống kê Dashboard: doanh thu, bàn, kho, biểu đồ 7 ngày |
| GET | `/baocao/...` | Các endpoint báo cáo doanh thu |

### 🤖 AI Copilot (`/api/AIChat`)

| Method | Endpoint | Mô Tả |
|:---:|:---|:---|
| POST | `/aichat/ask` | Hỏi đáp AI Copilot theo vai trò (kèm live data kinh doanh cho Quản lý) |
| POST | `/aichat/report` | Sinh báo cáo HTML từ AI dựa trên dữ liệu thực tế |
| GET | `/aichat/models` | Liệt kê các models Gemini đang available |

### 🏪 Cửa Hàng & Hồ Sơ

| Method | Endpoint | Mô Tả |
|:---:|:---|:---|
| GET/PUT | `/cuahang` | Xem và cập nhật thông tin cửa hàng |
| GET/PUT | `/profile` | Xem và cập nhật hồ sơ cá nhân |
| POST | `/webhook/...` | Nghiệp vụ Webhook nhận thông báo chuyển khoản ngân hàng |

---

## 🖥️ Hệ Thống Giao Diện (Views)

### Nhóm 1 — Trang Công Khai (Public)

| Route | View | Mô Tả |
|:---|:---|:---|
| `/` | `LandingPage.vue` | Trang chủ giới thiệu phần mềm POS36 |
| `/features` | `FeaturesView.vue` | Giới thiệu tính năng |
| `/pricing` | `PricingView.vue` | Bảng giá |
| `/about` | `AboutView.vue` | Về chúng tôi |
| `/solutions` | `SolutionsView.vue` | Giải pháp theo ngành |
| `/privacy` | `PrivacyView.vue` | Chính sách bảo mật |
| `/login` | `Login.vue` | Đăng nhập JWT |
| `/register` | `Register.vue` | Đăng ký cửa hàng mới |
| `/forgot-password` | `ForgotPasswordView.vue` | Quên mật khẩu → OTP Email → Đặt lại |

### Nhóm 2 — Giao Diện Quản Trị (`/admin/*`) 🔐

> Yêu cầu vai trò: **Admin, ChuCuaHang, QuanLy**  
> Shared layout: `AdminLayout.vue` (sidebar điều hướng)

| Route | View | Mô Tả |
|:---|:---|:---|
| `/admin` | `DashboardOverview.vue` | Bảng điều khiển tổng quan, biểu đồ doanh thu 7 ngày |
| `/admin/tables` | `TableSetup.vue` | Tạo/xóa chi nhánh, khu vực, bàn |
| `/admin/products` | `ProductSetup.vue` | Quản lý sản phẩm (CRUD, upload ảnh, danh mục) |
| `/admin/prices` | `PriceSetup.vue` | Thiết lập giá bán sản phẩm |
| `/admin/import-stock` | `ImportStock.vue` | Danh sách phiếu nhập kho |
| `/admin/import-create` | `CreateStock.vue` | Tạo phiếu nhập hàng mới |
| `/admin/employees` | `EmployeeSetup.vue` | Quản lý nhân viên, cấp tài khoản |
| `/admin/orders` | `OrderList.vue` | Lịch sử hóa đơn, xlx export, lọc tìm kiếm |
| `/admin/inventory` | `InventoryCheck.vue` | Danh sách phiếu kiểm kê |
| `/admin/inventory-create` | `InventoryCreate.vue` | Tạo phiếu kiểm kê, cân bằng kho |
| `/admin/cashbook` | `Cashbook.vue` | Sổ quỹ, xem phiếu thu/chi |
| `/admin/print-setup` | `PrintSetup.vue` | Tùy chỉnh mẫu in hóa đơn |
| `/admin/bank-setup` | `BankSetup.vue` | Cấu hình QR Code ngân hàng |
| `/admin/ai-report` | `AiReportView.vue` | Phân tích kinh doanh bằng AI |
| `/admin/daily-summary` | `DailySummary.vue` | Tổng kết ca / ngày |
| `/admin/sales-report` | `SalesReport.vue` | Báo cáo doanh thu |
| `/admin/profile` | `ProfileView.vue` | Hồ sơ cá nhân |
| `/admin/store-info` | `StoreInfoView.vue` | Thông tin và cài đặt cửa hàng |

### Nhóm 3 — Màn Hình Tác Nghiệp Fullscreen 🔐

| Route | View | Dành Cho | Mô Tả |
|:---|:---|:---|:---|
| `/pos` | `PosView.vue` | 💰 **Thu Ngân** | Màn hình POS: chọn bàn, xem món, áp mã giảm giá, thanh toán tiền mặt/QR |
| `/order` | `OrderView.vue` | 📝 **Phục Vụ** | Ghi nhận gọi món tại bàn, gửi xuống bếp, hủy món |
| `/kitchen` | `KitchenView.vue` | 🍳 **Bếp (KDS)** | Nhận order real-time, cập nhật trạng thái món "Đã xong" |

---

## 🔐 Phân Quyền & Luồng Điều Hướng

Hệ thống sử dụng **Vue Router Navigation Guards** kết hợp **JWT Claims** để bảo vệ routes.

```
User đăng nhập → Nhận JWT Token + VaiTro
       │
       ├── VaiTro = "Admin" / "ChuCuaHang" / "QuanLy"
       │         → Chuyển đến /admin (Dashboard quản trị)
       │
       ├── VaiTro = "ThuNgan"
       │         → Chuyển đến /pos (Màn hình thu ngân)
       │
       ├── VaiTro = "Order"
       │         → Chuyển đến /order (Màn hình gọi món)
       │
       └── VaiTro = "Bep"
                 → Chuyển đến /kitchen (Màn hình bếp)
```

**Quy tắc Navigation Guard:**
- Truy cập `/admin/*` mà không phải Admin/QuanLy/ChuCuaHang → tự động redirect về đúng màn hình theo vai trò
- Chưa đăng nhập mà vào trang cần auth → redirect về `/login`
- Token lưu tại: `localStorage["pos36_token"]`, vai trò tại `localStorage["pos36_role"]`

---

## 🔄 Luồng Hoạt Động Chi Tiết

### 1. Luồng Đăng Ký Cửa Hàng

```
Register.vue → POST /auth/register
                  │
                  ├── Tạo CuaHang (tên, SĐT)
                  ├── Tạo ChiNhanh mặc định ("Chi nhánh Trung tâm")
                  ├── Tạo KhuVuc mặc định ("Tầng 1")
                  ├── Tạo hồ sơ NhanVien (chủ cửa hàng)
                  └── Tạo TaiKhoan (VaiTro = "ChuCuaHang", mật khẩu BCrypt)
```

### 2. Luồng Quên Mật Khẩu (OTP Email)

```
ForgotPasswordView.vue
    │
    ├── Step 1: Nhập email → POST /auth/forgot-password
    │            └── Server tra email trong bảng NhanVien
    │                → Sinh OTP 6 số, lưu RAM Cache
    │                → Trả về {otp, tenDangNhap, tenNhanVien}
    │
    ├── Step 2: Vue dùng EmailJS gửi OTP đến email người dùng
    │
    ├── Step 3: Người dùng nhập OTP nhận được
    │
    └── Step 4: POST /auth/reset-password
                 └── Xác nhận OTP → Hash mật khẩu mới → Xóa OTP khỏi cache
```

### 3. Luồng Bán Hàng Chính (Order → Kitchen → POS)

```
[Nhân Viên Phục Vụ] OrderView.vue
    │
    ├── Chọn bàn → Load menu theo chi nhánh
    ├── Thêm các món vào giỏ
    └── Bấm "Gọi món" → POST /hoadon/goimon
                           │
                           ├── Kiểm tra/Tạo HoaDon cho bàn
                           ├── Thêm ChiTietHoaDon (TrangThaiMon = "Chờ chế biến")
                           ├── Cộng TongTien trên HoaDon
                           ├── Cập nhật TrangThai bàn = "Đang phục vụ"
                           └── SignalR Broadcast "CoDonHangMoi" → Bếp
                                              ↓
[Màn Hình Bếp] KitchenView.vue
    │
    ├── Nhận sự kiện "CoDonHangMoi" → Reload danh sách
    ├── GET /hoadon/bep/danh-sach (lọc TrangThaiMon = "Chờ chế biến")
    ├── Hiển thị: Tên bàn | Tên món | Số lượng | Ghi chú | Chờ X phút
    └── Bấm "✓ Xong" → PUT /hoadon/bep/xong/{chiTietId}
                           └── TrangThaiMon = "Đã Xong"
                           └── SignalR Broadcast "MonAnDaXong"
                                              ↓
[Thu Ngân] PosView.vue
    │
    ├── Xem sơ đồ bàn → Bấm bàn "Đang phục vụ"
    ├── GET /hoadon/ban/{banId} → Xem chi tiết hóa đơn
    ├── (Tùy chọn) Hủy món → POST /hoadon/huymon
    ├── (Tùy chọn) Chuyển bàn → POST /hoadon/chuyenban
    ├── (Tùy chọn) Ghép bàn → POST /hoadon/ghepban
    └── Thanh toán → POST /hoadon/thanhtoan/{banId}?phuongThuc=Tiền mặt
                        │
                        ├── HoaDon.TrangThai = "Đã thanh toán"
                        ├── Ban.TrangThai = "Trống"
                        ├── TonKho -= SoLuong từng món
                        ├── Tạo PhieuThuChi (LoaiPhieu="Thu")
                        └── SignalR Broadcast → Cập nhật màu sắc bàn trên tất cả màn hình
```

### 4. Luồng Thanh Toán QR Ngân Hàng (Real-time)

```
[Thu Ngân] PosView.vue
    │
    └── Chọn "Thanh toán QR" → SignalR gửi "YeuCauMoQR" (banId, soTien, maChungTu)
                                        ↓
[Nhân Viên] OrderView.vue
    │
    └── Nhận "NhanYeuCauMoQR" → Hiển thị QR Code ngân hàng cho khách quét
    └── Khách quét → Chuyển khoản → Webhook của ngân hàng POST /webhook/...
                                        ↓
WebhookController.cs
    │
    └── Phân tích nội dung CK → SignalR gửi "ThanhToanQRThanhCong"
                                        ↓
[Thu Ngân] PosView.vue
    └── Nhận signal → Tự động xác nhận thanh toán thành công
```

### 5. Luồng Nhập Kho

```
CreateStock.vue
    │
    ├── Chọn chi nhánh, tìm kiếm sản phẩm
    ├── Điền số lượng và giá nhập
    └── POST /nhaphang (TrangThai = "Hoàn thành")
            │
            ├── Tạo PhieuNhap + ChiTietPhieuNhap
            ├── TonKho += SoLuong cho mỗi sản phẩm
            └── Tạo PhieuThuChi (LoaiPhieu="Chi") nếu có TienThanhToan > 0
```

### 6. Luồng Kiểm Kê Kho

```
InventoryCreate.vue
    │
    ├── GET /kiemke/san-pham-ton-kho → Load danh sách SP + tồn kho hệ thống
    ├── Nhân viên đếm thực tế, điền vào "Số lượng kiểm kê"
    └── POST /kiemke (TrangThai = "Hoàn thành")
            │
            ├── Tạo PhieuKiemKe + ChiTietKiemKe (lưu cả tồn hệ thống vs thực tế)
            └── TonKho = SoLuongKiemKe (ghi đè hoàn toàn để cân bằng kho)
```

### 7. Luồng AI Copilot

```
AiReportView.vue / (Chatbox trong các view)
    │
    ├── Người dùng nhập câu hỏi
    └── POST /aichat/ask { question, role }
            │
            ├── Chọn System Prompt theo vai trò (Order/ThuNgan/QuanLy)
            ├── Nếu là Quản lý → Query DB lấy dữ liệu live (doanh thu hôm nay/tháng)
            ├── Gửi prompt tổng hợp đến Google Gemini 2.5 Flash API
            └── Trả về câu trả lời AI cho người dùng
```

---

## 📡 Hệ Thống Real-time (SignalR)

**Hub URL:** `ws://localhost:5198/kitchenHub`

### Các Sự Kiện SignalR

| Sự Kiện (Event) | Chiều | Mô Tả |
|:---|:---:|:---|
| `CoDonHangMoi` | Server → All | Có đơn gọi món mới / Có thanh toán xong → Bếp & tất cả reload bàn |
| `MonAnDaXong` | Server → All | Bếp xác nhận 1 món đã làm xong |
| `YeuCauThanhToan` | Client → All | Nhân viên yêu cầu thu ngân ra tính tiền cho bàn |
| `CoYeuCauThanhToan` | Server → All | Broadcast yêu cầu thanh toán |
| `SendOrderToKitchen` | Client → All | Gửi order trực tiếp qua SignalR (bổ sung) |
| `ReceiveNewOrder` | Server → All | Bếp nhận order mới |
| `YeuCauMoQR` | Client → All | Thu ngân yêu cầu mở QR thanh toán |
| `NhanYeuCauMoQR` | Server → All | Nhân viên nhận lệnh hiển thị QR cho khách |
| `HuyMoQR` | Client → All | Hủy yêu cầu QR (khách đổi ý) |
| `NhanHuyMoQR` | Server → All | Broadcast hủy QR |
| `XacNhanTienVe` | Client → All | Webhook báo chuyển khoản thành công |
| `ThanhToanQRThanhCong` | Server → All | Broadcast QR thanh toán thành công → Thu ngân auto-confirm |

---

## ⚙️ Cài Đặt & Khởi Chạy

### Yêu Cầu Hệ Thống

| Công Cụ | Phiên Bản | Link |
|:---|:---|:---|
| .NET SDK | 9.0+ | [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/9.0) |
| Node.js | LTS (18+) | [nodejs.org](https://nodejs.org/) |
| SQL Server | 2019+ | [Microsoft SQL Server](https://www.microsoft.com/sql-server) |
| Git | Latest | [git-scm.com](https://git-scm.com/) |

### Bước 1 — Clone Repository

```bash
git clone https://github.com/Nhanduc2912/POS36.git
cd POS36
```

### Bước 2 — Thiết Lập Cơ Sở Dữ Liệu

**Cách A — Dùng Script SQL (Nhanh nhất):**
```sql
-- Mở SQL Server Management Studio
-- Chạy file: Pos36DB.sql
```

**Cách B — EF Core Migration:**
```bash
cd POS36.Api
# Sửa ConnectionString trong appsettings.json trỏ đến SQL Server của bạn
dotnet ef database update
```

### Bước 3 — Cấu Hình Backend

Mở file `POS36.Api/appsettings.json` và điền thông tin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TEN_SERVER_CUA_BAN;Database=POS36_Db;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "GeminiAI": {
    "ApiKey": "NHẬP_GEMINI_API_KEY_CỦA_BẠN"
  },
  "Jwt": {
    "Key": "ChuoiBiMatCuaPOS36PhaiDaiVaKhoDoan1234567890!!!",
    "Issuer": "POS36Server",
    "Audience": "POS36Client"
  }
}
```

> 💡 Lấy Gemini API Key miễn phí tại: [Google AI Studio](https://aistudio.google.com/)

### Bước 4 — Khởi Chạy Backend

```bash
cd POS36.Api
dotnet restore
dotnet run
```

Backend sẽ chạy tại: `http://localhost:5198`  
Swagger UI: `http://localhost:5198/swagger`

### Bước 5 — Khởi Chạy Frontend

```bash
cd POS36.Web
npm install
npm run dev
```

Frontend sẽ chạy tại: `http://localhost:5173`

### Bước 6 — Đăng Ký & Sử Dụng

1. Truy cập `http://localhost:5173/register`
2. Điền tên cửa hàng, số điện thoại, tài khoản admin đầu tiên
3. Đăng nhập tại `/login` với tài khoản vừa tạo (vai trò: **ChuCuaHang**)
4. Vào `/admin` → thiết lập chi nhánh, khu vực, bàn, sản phẩm
5. Tạo tài khoản nhân viên tại `/admin/employees` với các vai trò phù hợp

---

## 📜 Ghi Chú Phát Triển

- **Serilog** được cấu hình với themed console log — terminal sẽ hiển thị đầy đủ: icon trạng thái, thời gian phản hồi, IP, thiết bị, tên đăng nhập cho mỗi request
- Tất cả thao tác phức tạp (thanh toán, nhập kho, kiểm kê) sử dụng **Database Transaction** (`BeginTransactionAsync`) để đảm bảo tính toàn vẹn dữ liệu
- **Cascade Delete** toàn bộ bị tắt (`DeleteBehavior.Restrict`) — phải xóa thủ công theo thứ tự để tránh mất dữ liệu nghiệp vụ
- File `ngrok.exe` đi kèm trong repo để dễ dàng tạo public URL demo/test

---

<div align="center">

Nếu bạn thấy dự án hữu ích, hãy để lại ⭐ trên GitHub!

**Tác giả:** [Nhanduc2912](https://github.com/Nhanduc2912)

</div>
