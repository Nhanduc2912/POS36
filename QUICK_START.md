# 🚀 Hướng Dẫn Khởi Động Nhanh POS36

## 📋 Mục Lục
- [Cài Đặt với Docker (Khuyến Nghị)](#-cài-đặt-với-docker-khuyến-nghị)
- [Cài Đặt Thủ Công](#-cài-đặt-thủ-công)
- [Cài Đặt Tự Động (Windows)](#-cài-đặt-tự-động-windows)
- [Tài Khoản Mặc Định](#-tài-khoản-mặc-định)
- [Các Lỗi Thường Gặp](#-các-lỗi-thường-gặp)

---

## 🐳 Cài Đặt với Docker (Khuyến Nghị)

### Bước 1: Cài Đặt Docker
- **Windows/Mac**: Tải [Docker Desktop](https://www.docker.com/products/docker-desktop)
- **Linux**: 
  ```bash
  curl -fsSL https://get.docker.com -o get-docker.sh
  sudo sh get-docker.sh
  ```

### Bước 2: Clone Repository
```bash
git clone https://github.com/your-repo/POS36.git
cd POS36
```

### Bước 3: Khởi Động
```bash
docker-compose up -d
```

### Bước 4: Khởi Tạo Database
Đợi 30 giây để SQL Server khởi động, sau đó:

**Windows PowerShell:**
```powershell
docker exec -it pos36-db /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "Pos36_Secret_Password_123!" -i /Pos36DB.sql
```

**Linux/Mac:**
```bash
docker exec -it pos36-db /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'Pos36_Secret_Password_123!' -i /Pos36DB.sql
```

### Bước 5: Truy Cập
- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:5098
- **Swagger**: http://localhost:5098/swagger

### Dừng Hệ Thống
```bash
docker-compose down
```

### Reset Dữ Liệu
```bash
docker-compose down -v
docker-compose up -d
```

---

## 🛠️ Cài Đặt Thủ Công

### Yêu Cầu
- ✅ .NET 9.0 SDK
- ✅ Node.js 18+
- ✅ SQL Server 2019+

### Bước 1: Cài Đặt Dependencies

#### Backend
```bash
cd POS36.Api
dotnet restore
```

#### Frontend
```bash
cd POS36.Web
npm install
```

### Bước 2: Cấu Hình Database

1. Tạo database trong SQL Server
2. Sửa `POS36.Api/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=POS36_Db;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
  }
}
```

3. Chạy migrations:
```bash
cd POS36.Api
dotnet ef database update
```

### Bước 3: Chạy Ứng Dụng

#### Terminal 1 - Backend
```bash
cd POS36.Api
dotnet run
```

#### Terminal 2 - Frontend
```bash
cd POS36.Web
npm run dev
```

### Truy Cập
- **Frontend**: http://localhost:5173
- **Backend**: http://localhost:5198
- **Swagger**: http://localhost:5198/swagger

---

## ⚡ Cài Đặt Tự Động (Windows)

### Bước 1: Cài Đặt Lần Đầu
1. Chuột phải vào `setup.bat`
2. Chọn **"Run as Administrator"**
3. Đợi quá trình cài đặt hoàn tất

### Bước 2: Chạy Hệ Thống
Double click vào `run.bat`

### Bước 3: Dừng Hệ Thống
Double click vào `stop.bat`

### Build Production
Double click vào `build.bat`

---

## 🔑 Tài Khoản Mặc Định

Sau khi đăng ký, bạn có thể tạo các tài khoản test:

### Chủ Cửa Hàng
```
Username: admin
Password: Admin123!
Vai trò: ChuCuaHang
```

### Quản Lý
```
Username: manager
Password: Manager123!
Vai trò: QuanLy
```

### Thu Ngân
```
Username: cashier
Password: Cashier123!
Vai trò: ThuNgan
```

### Phục Vụ
```
Username: waiter
Password: Waiter123!
Vai trò: Order
```

### Bếp
```
Username: kitchen
Password: Kitchen123!
Vai trò: Bep
```

---

## ❗ Các Lỗi Thường Gặp

### 1. Lỗi: "Cannot connect to SQL Server"

**Nguyên nhân**: SQL Server chưa khởi động hoặc connection string sai

**Giải pháp**:
```bash
# Kiểm tra SQL Server đang chạy
docker ps | grep pos36-db

# Hoặc trên Windows
services.msc → Tìm "SQL Server"
```

### 2. Lỗi: "Port 5198 already in use"

**Nguyên nhân**: Cổng đã được sử dụng bởi ứng dụng khác

**Giải pháp**:
```bash
# Windows
netstat -ano | findstr :5198
taskkill /PID <PID> /F

# Linux/Mac
lsof -i :5198
kill -9 <PID>
```

### 3. Lỗi: "npm ERR! code ELIFECYCLE"

**Nguyên nhân**: Node modules bị lỗi

**Giải pháp**:
```bash
cd POS36.Web
rm -rf node_modules package-lock.json
npm install
```

### 4. Lỗi: "Entity Framework migrations failed"

**Nguyên nhân**: Database chưa được tạo hoặc connection string sai

**Giải pháp**:
```bash
cd POS36.Api
dotnet ef database drop -f
dotnet ef database update
```

### 5. Lỗi: "CORS policy blocked"

**Nguyên nhân**: Frontend và Backend chạy trên domain khác nhau

**Giải pháp**: Kiểm tra `POS36.Api/Program.cs`:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});
```

### 6. Lỗi: "JWT Token invalid"

**Nguyên nhân**: Token hết hạn hoặc JWT Key không khớp

**Giải pháp**:
1. Đăng xuất và đăng nhập lại
2. Kiểm tra `appsettings.json` → `Jwt:Key` phải giống nhau giữa các lần chạy

### 7. Lỗi: "SignalR connection failed"

**Nguyên nhân**: Backend chưa khởi động hoặc URL sai

**Giải pháp**: Kiểm tra `POS36.Web/src/services/signalr.js`:
```javascript
const connection = new HubConnectionBuilder()
    .withUrl("http://localhost:5198/kitchenHub")
    .build();
```

---

## 📞 Hỗ Trợ

Nếu gặp vấn đề, vui lòng:
1. Kiểm tra [Issues](https://github.com/your-repo/POS36/issues)
2. Tạo issue mới với thông tin chi tiết
3. Liên hệ: support@pos36.com

---

## 🎯 Bước Tiếp Theo

Sau khi cài đặt thành công:

1. ✅ Đọc [README_ENHANCED.md](README_ENHANCED.md) để hiểu chi tiết hệ thống
2. ✅ Xem [API Documentation](http://localhost:5198/swagger)
3. ✅ Tạo tài khoản test và khám phá các tính năng
4. ✅ Cấu hình Google Gemini API Key cho AI Copilot
5. ✅ Thiết lập QR Code ngân hàng cho thanh toán

---

<div align="center">

**Happy Coding! 🚀**

[⬆ Back to Top](#-hướng-dẫn-khởi-động-nhanh-pos36)

</div>
