_Mục tiêu: Đảm bảo không bao giờ xảy ra rò rỉ dữ liệu chéo giữa các quán và tự động xử lý khi hết hạn thuê._

- [ ] **Task 1.1: Global Query Filter (Lưới lọc dữ liệu tàng hình)**
  - Cấu hình file `Data/AppDbContext.cs` để tự động bắt `CuaHangId` từ Token Claims (người dùng hiện tại).
  - Ép tất cả các truy vấn `SELECT`, `UPDATE`, `DELETE` trên toàn bộ 20 bảng hệ thống phải tự động lọc ngầm theo ID cửa hàng mà không cần lập trình viên gõ mệnh đề `WHERE` thủ công.
- [ ] **Task 1.2: Background Service "Thần Chết" (Subscription Worker)**
  - Viết một class kế thừa từ `BackgroundService` hoặc `IHostedService` trong .NET Core chạy ngầm 24/7.
  - Định thời đúng **00:00:01 mỗi ngày**, Worker sẽ tự động quét bảng `CuaHangs` và cập nhật các cửa hàng có `NgayHetHan < DateTime.Now` chuyển sang trạng thái `'HetHan'`.
- [ ] **Task 1.3: Middleware Chặn Đăng Nhập & Thao Tác**
  - Cập nhật luồng Auth: Nếu trạng thái cửa hàng là `HetHan`, chặn mọi quyền tạo hóa đơn, tạo đơn hàng mới, chỉ cho phép xem Dashboard và mở màn hình yêu cầu thanh toán gia hạn.

### 📌 GIAI ĐOẠN 2: THIẾT KẾ BỘ CÔNG CỤ AI AGENT (GEMINI FUNCTION CALLING)

_Mục tiêu: Định nghĩa các khả năng "hành động" và nạp "quyền lực" điều khiển hệ thống cho AI Agent._

- [ ] **Task 2.1: Xây dựng SuperAdminController**
  - Viết các API bảo mật (Chỉ tài khoản của Sếp có Role = `SuperAdmin` mới gọi được).
  - Các Endpoint bao gồm: Xem doanh thu tổng của toàn bộ hệ thống SaaS, Danh sách các quán sắp hết hạn, Kích hoạt thủ công/Mở khóa cho một quán, Cập nhật thông số gói dịch vụ.
- [ ] **Task 2.2: Khai báo Danh mục AI Tools (Function Schemas)**
  - Định nghĩa danh sách các hàm (Tools) dưới dạng JSON Schema để gửi lên Google AI Studio (Gemini API).
  - _Ví dụ mẫu:_ Hàm `ThongKeSaaS()`, Hàm `KhoaCuaHang(string shopId)`, Hàm `MoKhoaCuaHang(string shopId)`, Hàm `GiaHanGoi(string shopId, int months)`.
- [ ] **Task 2.3: Luồng Xử Lý Bất Đồng Bộ "Human-In-The-Loop"**
  - Cấu hình logic Backend nhận prompt từ sếp -> Đẩy lên Gemini.
  - Nếu câu lệnh là Thêm/Sửa/Xóa cấu hình nhạy cảm, Backend sẽ không thực thi ngay mà đóng gói trạng thái trả về Client dạng `{"status": "RequiresAction", "function": "KhoaCuaHang", "args": {"shopId": "cafemixi"}}`.

### 📌 GIAI ĐOẠN 3: GIAO DIỆN CHIA ĐÔI MÀN HÌNH VS CODE (FRONTEND VUE 3)

_Mục tiêu: Trải nghiệm UX đỉnh cao, chia đôi workspace kéo dãn linh hoạt, tích hợp CLI Terminal._

- [ ] **3.1. Main Layout Split Window (`SuperAdminLayout.vue`)**
  - Thiết kế cấu trúc phân lớp: Khu vực nội dung chính quản trị nằm trên (hiển thị biểu đồ dòng tiền, danh sách quán), Khu vực AI Terminal nằm dưới.
- [ ] **3.2. Cài đặt Resizer Bar (Kéo thả thuần bằng JS)**
  - Sử dụng sự kiện chuột `mousedown`, `mousemove`, `mouseup` để tính toán độ cao linh hoạt của Terminal dựa trên toạ độ con trỏ chuột so với viewport đáy.
  - Đảm bảo độ mượt mà cao, không giật lag, tự động co giãn không gian slot bên trên mà không sử dụng thư viện cồng kềnh từ bên ngoài.
- [ ] **3.3. UI Terminal Dashboard (Màn hình Hacker đen)**
  - Thiết kế giao diện Terminal mã nguồn với màu nền `#1e1e1e`, font chữ hệ thống `Monospace` hoặc `Courier New`.
  - Chia hệ thống Tab chuẩn VS Code: `TERMINAL AI`, `OUTPUT SAAS CLAIM`, `LOG AUTO-CRON`.
  - Input dòng lệnh nằm sát gầm với prompt prefix biểu tượng `pos36-super$`.

### 📌 GIAI ĐOẠN 4: LIÊN KẾT LUỒNG CHẤP THUẬN LỆNH (ACCEPTANCE WORKFLOW)

_Mục tiêu: Đảm bảo an toàn tuyệt đối, AI chỉ thực thi khi sếp phê duyệt (Claude Code / Gemini CLI Style)._

- [ ] **4.1. Bắt phản hồi Trạng thái Phê duyệt**
  - Viết service Axios đón đầu kết quả trả về từ `AIChatController`. Nếu phát hiện payload mang cờ `RequiresAction`, lập tức treo luồng nhập và vẽ ra một box cảnh báo màu vàng viền mờ.
- [ ] **4.2. Render Component Phê Duyệt lệnh**
  - Hiển thị thông tin tường minh: _Tên hàm AI muốn chạy_, _Toàn bộ tham số đầu vào được bóc tách từ ngôn ngữ tự nhiên_.
  - Render 2 nút chức năng hoặc phím tắt: `Accept (Y)` và `Reject (N)`.
- [ ] **4.3. Gọi API Thực thi & Update UI**
  - **Nếu chọn Accept:** Gửi lệnh xác nhận lên Backend để C# kích hoạt Transaction SQL thực thi vào Database thật -> Trả log màu xanh lá báo thành công.
  - **Nếu chọn Reject:** Hủy lệnh, xóa hàng đợi, thông báo lên Terminal: `[Aborted] Lệnh đã bị hủy bỏ an toàn theo yêu cầu của Super-Admin.`

---

## 💡 CÁCÝ TƯỞNG PHÁT TRIỂN MỞ RỘNG TIỀN NĂNG (GIAI ĐOẠN TRƯỞNG THÀNH)

Khi hệ thống SaaS đã vận hành ổn định và sếp muốn nhân rộng quy mô thương mại, các tính năng sau sẽ giúp tăng giá trị sản phẩm lên gấp nhiều lần:

1. **Hệ sinh thái tự gọi món tại bàn (QR Order Agent):** Khách quét QR tại bàn -> Menu hiển thị -> Khách tự chọn món và gửi đơn -> Tự động đồng bộ thời gian thực xuống `KitchenView` (Màn hình Bếp) giúp chủ quán tiết kiệm 50% chi phí nhân sự phục vụ.
2. **Cổng thanh toán tự động Webhook (Auto-Billing):** Kết nối sâu với API ngân hàng (VietQR/SePay/Casso). Khi chủ quán quét mã gia hạn, hệ thống tự động cộng thêm thời gian chạy mà sếp không cần can thiệp thủ công.
3. **AI Dự báo Nguyên vật liệu:** Tận dụng dữ liệu tiêu thụ hàng ngày, AI sẽ phân tích và cảnh báo chủ quán: _"Dựa trên dữ liệu 7 ngày qua, dự kiến ngày mai quán sẽ hết thịt bò, bạn nên tạo phiếu nhập kho ngay."_

---

_Kế hoạch được phê duyệt và theo dõi tiến độ nội bộ bởi Super-Admin POS36._
