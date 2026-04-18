# 📋 POS36 — Kiểm Định Toàn Diện Hệ Thống

> Phiên bản kiểm tra: 2026-04-18 | Môn: Phát triển ứng dụng .NET — FPT Polytechnic

---

## 🔴 PHẦN 1: CÁC BUG & LỖI LOGIC ĐÃ TÌM RA

### Bug #1 — `BanController.cs`: Thiếu `[Authorize]`
**Mức độ:** 🔴 Nghiêm trọng  
**Vị trí:** `BanController.cs` — toàn bộ controller  
**Mô tả:** Controller quản lý bàn **không có `[Authorize]`** → bất kỳ ai cũng có thể gọi API thêm bàn, chuyển bàn mà không cần đăng nhập.  
**Hướng fix:** Thêm `[Authorize]` vào đầu class.

---

### Bug #2 — `AuthController.cs`: OTP lưu trong RAM — mất khi restart
**Mức độ:** 🔴 Nghiêm trọng (về bảo mật)  
**Trạng thái:** ✅ **ĐÃ FIX** (2026-04-18)  
**Vị trí:** `AuthController.cs` — dòng 22, 159, 164  
**Mô tả:**
```csharp
private static readonly Dictionary<string, string> _otpCache = new Dictionary<string, string>();
```
- OTP lưu trong RAM → **mất toàn bộ** khi server restart.
- OTP **không có thời gian hết hạn** → ai đó yêu cầu OTP từ hôm qua vẫn dùng được mãi.
- **Lỗi bảo mật nghiêm trọng:** Backend trả OTP thẳng về response JSON → Frontend đọc và gửi email. Điều này có nghĩa là **bất kỳ ai intercept HTTP response đều biết OTP**.

**Fix đã áp dụng:**
- Tạo model `OtpRequest` với `ExpiresAt`, `OtpHash`, `DaSDung`.
- Lưu OTP vào bảng `OtpRequests` trong DB — không còn mất khi restart.
- OTP có **TTL 5 phút** — tự hết hạn.
- OTP được **hash bằng bcrypt** — không lưu plain text.
- `DaSDung = true` sau khi dùng — chống **replay attack**.
- ⚠️ OTP tạm thời vẫn trả về response (để tương thích frontend EmailJS). TODO: Xóa khi có SMTP.

**Hướng fix hoàn chỉnh tiếp theo:** Lưu OTP vào DB với `ExpiresAt = DateTime.Now.AddMinutes(5)`, backend tự gửi email (dùng SMTP/SendGrid).

---

### Bug #3 — `BaoCaoController.cs`: Báo cáo dùng bảng `ThanhToans` nhưng hóa đơn không ghi vào đó
**Mức độ:** 🟠 Quan trọng  
**Vị trí:** `BaoCaoController.cs` — dòng 59–66  
**Mô tả:**
```csharp
var thongKePhuongThuc = await _context.ThanhToans
    .Where(t => listHoaDonId.Contains(t.HoaDonId))...
```
Nhưng trong `HoaDonController.ThanhToan()`, dữ liệu được lưu vào bảng **`PhieuThuChis`**, không phải `ThanhToans`. Kết quả: **báo cáo phương thức thanh toán luôn trả về rỗng**.

**Hướng fix:** Sửa query sang `PhieuThuChis` hoặc bổ sung logic ghi vào `ThanhToans` sau khi thanh toán.

---

### Bug #4 — `DashboardController.cs`: Doanh thu 7 ngày dùng `NgayTao` thay `NgayThanhToan`
**Mức độ:** 🟠 Quan trọng  
**Vị trí:** `DashboardController.cs` — dòng 78  
```csharp
.Where(h => ... && h.NgayTao >= sevenDaysAgo)
```
**Mô tả:** Biểu đồ doanh thu 7 ngày lọc theo `NgayTao` (ngày mở bàn) thay vì `NgayThanhToan` → hóa đơn thanh toán tối nay nhưng được mở từ hôm qua bị **tính sai ngày**.

**Hướng fix:** Đổi thành `h.NgayThanhToan >= sevenDaysAgo`.

---

### Bug #5 — `NhapHangController.cs`: Phiếu nhập "Đang xử lý" vẫn trừ tiền quỹ
**Mức độ:** 🟠 Quan trọng  
**Trạng thái:** ✅ **ĐÃ FIX** (2026-04-18)  
**Vị trí:** `NhapHangController.cs` — dòng 137  
```csharp
if (request.TrangThai == "Hoàn thành" && request.TienThanhToan > 0)
```
Logic này đúng. **Nhưng:** Nếu frontend gửi trạng thái `"Hoàn thành"` và tiền thanh toán 0, tồn kho vẫn tăng nhưng không ghi phiếu chi → **thiếu audit trail tài chính**.

Ngoài ra, **không có API để cập nhật trạng thái phiếu nhập** (từ "Đang xử lý" → "Hoàn thành"), dẫn đến:
- Frontend chỉ tạo được một lần, không đổi trạng thái sau.
- Tồn kho không tăng nếu lưu trạng thái "Đang xử lý".

**Fix đã áp dụng:** Thêm API `PUT /api/NhapHang/{id}/xacnhan` để:
- Chuyển trạng thái "Đang xử lý" → "Hoàn thành"
- Tăng tồn kho cho tất cả sản phẩm trong phiếu
- Tự động ghi `PhieuThuChi` loại Chi để đảm bảo audit trail

---

### Bug #6 — `HoaDonController.GoiMon()`: Không validate tồn kho trước khi đặt món
**Mức độ:** 🟡 Trung bình  
**Vị trí:** `HoaDonController.cs` — dòng 72–90  
**Mô tả:** Cho phép gọi món ngay cả khi sản phẩm đã **hết tồn kho** (`TonKho = 0` hoặc âm). Hệ thống không kiểm tra `TonKhos` trước khi thêm `ChiTietHoaDon`.

**Hướng fix:** Kiểm tra tồn kho tại chi nhánh trước khi tạo chi tiết, báo lỗi nếu không đủ hàng.

---

### Bug #7 — `KiemKeController.cs`: Lặp comment thừa
**Mức độ:** 🟢 Nhỏ  
**Trạng thái:** ✅ **ĐÃ FIX** — Comment trùng không tồn tại trong code nguồn (đã được xóa trước đó)

---

### Bug #8 — `ThuChiController.cs`: `GetCuaHangId()` fallback về `"1"` — security hole
**Mức độ:** 🟡 Trung bình  
**Vị trí:** `ThuChiController.cs`, `NhapHangController.cs`, `KiemKeController.cs` — dòng `?.Value ?? "1"`  
```csharp
private int GetCuaHangId() => int.Parse(User.FindFirst("CuaHangId")?.Value ?? "1");
```
Nếu claim bị thiếu, mặc định trả về `1` (cửa hàng đầu tiên). Điều này có thể rò rỉ dữ liệu của cửa hàng khác trong trường hợp lỗi token.  
**Hướng fix:** Throw exception nếu claim không tồn tại (như các controller khác đã làm).

---

### Bug #9 — `SanPhamController.cs`: Xóa sản phẩm không kiểm tra hóa đơn đang phục vụ
**Mức độ:** 🟡 Trung bình  
**Vị trí:** `SanPhamController.cs` — `DeleteSanPham()`  
**Mô tả:** Có thể xóa sản phẩm đang được gọi trong `ChiTietHoaDon` của hóa đơn **Đang phục vụ** → hóa đơn bị mất thông tin tên món.

**Hướng fix:** Kiểm tra `ChiTietHoaDons.Any(ct => ct.SanPhamId == id && ct.HoaDon.TrangThai == "Đang phục vụ")` trước khi cho xóa.

---

### Bug #10 — `HoaDonController.GhepBan()`: Không merge `KhachHangId`
**Mức độ:** 🟡 Trung bình  
**Vị trí:** `HoaDonController.cs` — dòng 217–225  
**Mô tả:** Khi ghép bàn, chỉ chuyển `ChiTietHoaDons` và cộng `TongTien`. Nếu bàn gốc đã ghim khách hàng (`KhachHangId`), thông tin này bị mất → không tích điểm cho đúng khách.

---

### Bug #11 — `BanController.TaoBanNhanh()`: Tự tạo tên bàn trùng
**Mức độ:** 🟡 Trung bình  
**Vị trí:** `BanController.cs` — dòng 76  
```csharp
int soBanHienTai = await _context.Bans.CountAsync(b => b.KhuVucId == request.KhuVucId && b.CuaHangId == cuaHangId);
```
Đếm theo **số lượng** không phải số bàn lớn nhất → nếu đã xóa bàn 3 mà còn bàn 1, 2, 4 (count = 3), bàn mới sẽ tạo "Bàn 4" — **trùng** với bàn đang có.

---

### Bug #12 — Không có phân quyền theo vai trò trên API
**Mức độ:** 🟠 Quan trọng  
**Trạng thái:** ✅ **ĐÃ FIX** (2026-04-18)  
**Mô tả:** Nhiều API chỉ check `[Authorize]` mà không check `[Authorize(Roles = "ChuCuaHang")]`. Nhân viên (Thu ngân, Phục vụ) có thể gọi API xóa sản phẩm, xóa nhân viên, thay đổi thiết lập,...

**Fix đã áp dụng — thêm `[Authorize(Roles = "ChuCuaHang")]` cho:**
- `SanPhamController`: `CreateSanPham`, `UpdateSanPham`, `DeleteSanPham`, `UpdatePrice`
- `NhanVienController`: `Create`, `Update`, `Delete`
- `DanhMucController`: `CreateDanhMuc`, `UpdateDanhMuc`, `DeleteDanhMuc`
- `ThietLapController`: `CreateChiNhanh`, `CreateKhuVuc`, `CreateBan`, `SaveThietLap`
- `BanController`: `Create`, `TaoBanNhanh`
- `ThuChiController`: `GetDanhSach` (sổ quỹ — dữ liệu tài chính nhạy cảm)

---

## 🟡 PHẦN 2: CHỨC NĂNG CHƯA LÀM / CÒN THIẾU

| # | Chức Năng | Mô Tả Thiếu |
|---|-----------|-------------|
| 1 | **Cập nhật trạng thái Phiếu Nhập** | Không có API PUT để đổi "Đang xử lý" → "Hoàn thành" và tăng tồn kho |
| 2 | **Thêm chi phí thủ công vào Sổ Quỹ** | `ThuChiController` chỉ có `GET`, không có `POST` tạo phiếu chi |
| 3 | **Lịch sử điểm khách hàng** | Không ghi `LichSuDiem` — không biết khách đã dùng điểm ở đơn nào |
| 4 | **Báo cáo doanh thu theo sản phẩm** | `BaoCaoController` chỉ có Top 5 món bán chạy và tổng tiền |
| 5 | **Thống kê chi phí nhập hàng** | Không có báo cáo tổng tiền nhập hàng vs doanh thu (lợi nhuận thô) |
| 6 | **Xuất Excel / PDF báo cáo** | Không có tính năng xuất file |
| 7 | **Quản lý nhà cung cấp** | Bảng `NhaCungCap` bị xóa, phiếu nhập chỉ ghi "Nhà cung cấp lẻ" |
| 8 | **Đặt lịch / Đặt bàn trước** | Không có tính năng reservation |
| 9 | **Khóa ca làm việc (Ca/Shift)** | Không phân biệt ca sáng/tối, không đối tiền cuối ca |
| 10 | **Hủy hóa đơn / Hoàn tiền** | Không có API hủy đơn khi khách đã thanh toán rồi đổi ý |
| 11 | **Giảm giá / Voucher** | Không có cơ chế giảm giá trực tiếp trên hóa đơn (ngoài điểm) |
| 12 | **Combo / Set món** | Không có tính năng đặt combo (nhiều món = 1 giá) |

---

## 🔵 PHẦN 3: TÍNH NĂNG NÂNG CAO KHẢ THI — ĐỀ XUẤT

### 🥇 Nhóm 1 — Khả thi cao, ít công sức, tác động lớn

#### A. Thêm chi phí thủ công vào Sổ Quỹ
**Mục tiêu:** Thu ngân/Chủ có thể ghi phiếu chi tay (điện, nước, lương,...).  
**Cách làm:** Thêm `POST /api/ThuChi` nhận `{loai, lyDo, giaTri, phuongThuc}` và lưu vào `PhieuThuChis`.  
**Thời gian ước tính:** 1–2 giờ.

#### B. Xem lịch sử điểm của khách hàng
**Mục tiêu:** Trong trang DetailKhachHang, hiện danh sách lần nào tích/tiêu điểm.  
**Cách làm:** Thêm bảng `LichSuDiem (KhachHangId, HoaDonId, LoaiDiem, SoDiem, NgayGiaoDich)` và ghi vào khi thanh toán.  
**Thời gian ước tính:** 3–4 giờ.

#### C. Cập nhật trạng thái Phiếu Nhập
**Mục tiêu:** Xác nhận hàng đã về → tồn kho tăng.  
**Cách làm:** Thêm `PUT /api/NhapHang/{id}/xacnhan` để đổi trạng thái và tăng tồn kho.  
**Thời gian ước tính:** 1–2 giờ.

---

### 🥈 Nhóm 2 — Tính năng nghiệp vụ quan trọng

#### D. Hủy hóa đơn đang phục vụ
**Mục tiêu:** Khách không muốn ăn nữa → hủy toàn bộ order, trả bàn về Trống.  
**Cách làm:** API `POST /api/HoaDon/huy/{banId}` → đặt `TrangThai = "Đã hủy"`, reset bàn.  
**Thời gian ước tính:** 1–2 giờ.

#### E. Cơ chế giảm giá trực tiếp (%)
**Mục tiêu:** Thu ngân có thể áp dụng giảm giá % hoặc giảm số tiền cố định trên hóa đơn.  
**Cách làm:** Thêm params `giamGia` (% hoặc cố định) vào `ThanhToan`, tính lại `TongTien`, ghi rõ vào `PhieuThu`.  
**Thời gian ước tính:** 2–3 giờ.

#### F. Phân quyền theo vai trò trên API
**Mục tiêu:** Nhân viên không xóa được sản phẩm, không xem sổ quỹ,...  
**Cách làm:** Thêm `[Authorize(Roles = "ChuCuaHang")]` vào các endpoint nhạy cảm.  
**Thời gian ước tính:** 1 giờ.

---

### 🥉 Nhóm 3 — Tính năng nâng cao

#### G. Báo cáo lợi nhuận thô
**Mục tiêu:** Dashboard hiện Doanh thu − Chi phí nhập hàng = Lợi nhuận.  
**Cách làm:** Bổ sung query vào `DashboardController` lấy tổng `PhieuThuChis` loại Chi.  
**Thời gian ước tính:** 2–3 giờ.

#### H. Thông báo khi tồn kho xuống thấp
**Mục tiêu:** Khi `TonKho <= 5`, gửi cảnh báo real-time qua SignalR đến Admin.  
**Cách làm:** Sau mỗi lần thanh toán thành công, kiểm tra tồn kho và push `SignalR` message.  
**Thời gian ước tính:** 2 giờ.

#### I. Xuất Excel báo cáo ngày
**Mục tiêu:** Chủ quán xuất file Excel tổng kết ngày.  
**Cách làm:** Cài package `ClosedXML`, thêm endpoint `GET /api/BaoCao/export-excel`.  
**Thời gian ước tính:** 3–4 giờ.

---

## 📊 PHẦN 4: MỤC TIÊU CHẤT LƯỢNG CẦN HƯỚNG TỚI

| Tiêu chí | Hiện tại | Mục tiêu |
|----------|----------|---------|
| Bảo mật API | `[Authorize]` cơ bản | Phân quyền theo Role |
| OTP Quên mật khẩu | Lưu RAM, gửi qua frontend | Lưu DB có TTL, backend gửi email |
| Audit trail | Ghi log Serilog | Ghi lịch sử thay đổi điểm, tồn kho |
| Báo cáo | Doanh thu + Top 5 món | Thêm lợi nhuận, chi phí |
| Tồn kho | Trừ sau thanh toán | Cảnh báo gần hết, lock khi hết |
| Validation | Cơ bản ở vài controller | Đồng bộ toàn bộ, dùng `DataAnnotations` |

---

## 🗺️ PHẦN 5: ĐỀ XUẤT THỨ TỰ LÀM

```
Ưu tiên 1 (Bug nghiêm trọng):
  ✅ [Bug#1] Thêm [Authorize] cho BanController
  ✅ [Bug#2] Fix OTP lưu DB có TTL + hash bcrypt (2026-04-18)
  ✅ [Bug#3] Fix báo cáo phương thức thanh toán (PhieuThuChis thay ThanhToans)
  ✅ [Bug#4] Fix dashboard dùng NgayThanhToan

Ưu tiên 2 (Logic nghiệp vụ):
  ✅ [Bug#12] Phân quyền API theo Role — SanPham, NhanVien, DanhMuc, ThietLap, Ban, ThuChi (2026-04-18)
  ▶ [F] Cơ chế giảm giá trực tiếp  
  ▶ [D] Hủy hóa đơn

Ưu tiên 3 (Tính năng còn thiếu):
  ▶ [A] Thêm phiếu chi thủ công
  ✅ [C] Xác nhận phiếu nhập → tăng tồn kho — API PUT /api/NhapHang/{id}/xacnhan (2026-04-18)
  ▶ [B] Lịch sử điểm khách hàng

Ưu tiên 4 (Nâng cao):
  ▶ [G] Báo cáo lợi nhuận thô
  ▶ [H] Cảnh báo tồn kho real-time
  ▶ [I] Xuất Excel
```
