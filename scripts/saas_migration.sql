-- =============================================
-- POS36 SaaS Migration Script
-- Chạy script này sau khi đã chạy EF Migration
-- =============================================

-- 1. Thêm cột mới vào bảng TaiKhoans (nếu chưa có)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TaiKhoans') AND name = 'Email')
BEGIN
    ALTER TABLE TaiKhoans ADD Email NVARCHAR(MAX) NOT NULL DEFAULT '';
    ALTER TABLE TaiKhoans ADD SoDienThoai NVARCHAR(MAX) NOT NULL DEFAULT '';
    ALTER TABLE TaiKhoans ADD NgayTao DATETIME2 NOT NULL DEFAULT GETDATE();
    ALTER TABLE TaiKhoans ADD IsActive BIT NOT NULL DEFAULT 1;
    ALTER TABLE TaiKhoans ADD LanDangNhapCuoi DATETIME2 NULL;
END
GO

-- 2. Thêm cột mới vào bảng CuaHangs (nếu chưa có)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CuaHangs') AND name = 'TrangThai')
BEGIN
    ALTER TABLE CuaHangs ADD Email NVARCHAR(MAX) NULL;
    ALTER TABLE CuaHangs ADD DiaChi NVARCHAR(MAX) NULL;
    ALTER TABLE CuaHangs ADD LogoUrl NVARCHAR(MAX) NULL;
    ALTER TABLE CuaHangs ADD TrangThai NVARCHAR(MAX) NOT NULL DEFAULT 'DungThu';
    ALTER TABLE CuaHangs ADD NgayHetHan DATETIME2 NOT NULL DEFAULT DATEADD(DAY, 7, GETDATE());
    ALTER TABLE CuaHangs ADD GoiDichVu NVARCHAR(MAX) NULL;
    ALTER TABLE CuaHangs ADD GhiChu NVARCHAR(MAX) NULL;
END
GO

-- 3. Tạo bảng GoiDichVus
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GoiDichVus')
BEGIN
    CREATE TABLE GoiDichVus (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        TenGoi NVARCHAR(MAX) NOT NULL,
        MaGoi NVARCHAR(MAX) NOT NULL,
        SoThang INT NOT NULL,
        GiaThang DECIMAL(18,2) NOT NULL,
        TongGia DECIMAL(18,2) NOT NULL,
        GioiHanHoaDon INT NOT NULL DEFAULT 0,
        GioiHanNhanVien INT NOT NULL DEFAULT 0,
        MoTa NVARCHAR(MAX) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        ThuTuHienThi INT NOT NULL DEFAULT 0
    );
END
GO

-- 4. Tạo bảng LichSuDangKys
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LichSuDangKys')
BEGIN
    CREATE TABLE LichSuDangKys (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CuaHangId INT NOT NULL,
        GoiDichVuId INT NOT NULL,
        NgayBatDau DATETIME2 NOT NULL,
        NgayKetThuc DATETIME2 NOT NULL,
        SoTienThanhToan DECIMAL(18,2) NOT NULL,
        TrangThai NVARCHAR(MAX) NOT NULL DEFAULT 'ChoThanhToan',
        PhuongThucThanhToan NVARCHAR(MAX) NULL,
        MaGiaoDich NVARCHAR(MAX) NULL,
        GhiChu NVARCHAR(MAX) NULL,
        NgayTao DATETIME2 NOT NULL DEFAULT GETDATE(),
        NgayThanhToan DATETIME2 NULL,
        NguoiDuyet NVARCHAR(MAX) NULL,
        FOREIGN KEY (CuaHangId) REFERENCES CuaHangs(Id),
        FOREIGN KEY (GoiDichVuId) REFERENCES GoiDichVus(Id)
    );
END
GO

-- 5. Tạo bảng LuotTruyCaps
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LuotTruyCaps')
BEGIN
    CREATE TABLE LuotTruyCaps (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CuaHangId INT NULL,
        TaiKhoanId INT NULL,
        IpAddress NVARCHAR(MAX) NOT NULL DEFAULT '',
        UserAgent NVARCHAR(MAX) NOT NULL DEFAULT '',
        Url NVARCHAR(MAX) NOT NULL DEFAULT '',
        ThietBi NVARCHAR(MAX) NULL,
        ThoiGian DATETIME2 NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (CuaHangId) REFERENCES CuaHangs(Id)
    );
END
GO

-- 6. Tạo bảng ThongBaoHeThongs
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ThongBaoHeThongs')
BEGIN
    CREATE TABLE ThongBaoHeThongs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CuaHangId INT NULL,
        TieuDe NVARCHAR(MAX) NOT NULL,
        NoiDung NVARCHAR(MAX) NOT NULL,
        LoaiThongBao NVARCHAR(MAX) NOT NULL DEFAULT 'ThongTin',
        DaDoc BIT NOT NULL DEFAULT 0,
        NgayTao DATETIME2 NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (CuaHangId) REFERENCES CuaHangs(Id)
    );
END
GO

-- =============================================
-- SEED DATA: 3 Gói dịch vụ mặc định
-- =============================================
IF NOT EXISTS (SELECT * FROM GoiDichVus WHERE MaGoi = 'STARTER')
BEGIN
    INSERT INTO GoiDichVus (TenGoi, MaGoi, SoThang, GiaThang, TongGia, GioiHanHoaDon, GioiHanNhanVien, MoTa, IsActive, ThuTuHienThi)
    VALUES 
        (N'Gói Starter', 'STARTER', 6, 150000, 900000, 200, 2, N'Phù hợp cho quán nhỏ mới bắt đầu. Giới hạn 200 hóa đơn/tháng và 2 nhân viên.', 1, 1),
        (N'Gói Pro', 'PRO', 12, 125000, 1500000, 0, 0, N'Không giới hạn tính năng, phù hợp cho quán trung bình trở lên.', 1, 2),
        (N'Gói Ultimate', 'ULTIMATE', 24, 116667, 2800000, 0, 0, N'Tiết kiệm nhất! Không giới hạn, ưu tiên hỗ trợ kỹ thuật.', 1, 3);
END
GO

-- =============================================
-- SEED DATA: Tài khoản SuperAdmin mặc định
-- Mật khẩu: SuperAdmin@123 (BCrypt hash)
-- =============================================

-- Tạo 1 CuaHang giả cho SuperAdmin (CuaHangId = 0 hoặc tạo mới)
IF NOT EXISTS (SELECT * FROM TaiKhoans WHERE VaiTro = 'SuperAdmin')
BEGIN
    -- Tạo cửa hàng hệ thống
    INSERT INTO CuaHangs (TenCuaHang, SoDienThoai, NgayDangKy, TrangThai, NgayHetHan)
    VALUES (N'POS36 System', '0000000000', GETDATE(), 'HoatDong', DATEADD(YEAR, 100, GETDATE()));

    DECLARE @SystemCuaHangId INT = SCOPE_IDENTITY();

    -- Tạo tài khoản SuperAdmin
    -- Hash của 'SuperAdmin@123' bằng BCrypt (cost 11)
    INSERT INTO TaiKhoans (CuaHangId, TenDangNhap, MatKhauHash, VaiTro, Email, SoDienThoai, NgayTao, IsActive)
    VALUES (@SystemCuaHangId, 'superadmin', '$2a$11$LBQqxGBne2EsjPBOhqNzouPKKnHIvFPlR0ByMwXdD6JXXzTcUbf4.', 'SuperAdmin', 'admin@pos36.vn', '0000000000', GETDATE(), 1);
    
    PRINT N'✅ Tài khoản SuperAdmin đã được tạo: superadmin / SuperAdmin@123';
END
GO

-- Cập nhật các cửa hàng hiện tại (nếu chưa có TrangThai)
UPDATE CuaHangs SET TrangThai = 'HoatDong', NgayHetHan = DATEADD(MONTH, 12, GETDATE())
WHERE TrangThai = 'DungThu' AND TenCuaHang != 'POS36 System';
GO

PRINT N'✅ Migration SaaS hoàn tất!';
GO
