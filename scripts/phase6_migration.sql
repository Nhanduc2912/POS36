-- ============================================
-- POS36 Phase 6: Soft Delete Migration
-- Chạy script này trong SQL Server Management Studio
-- hoặc dùng: dotnet ef migrations add Phase6_SoftDelete
-- ============================================

-- 1. Thêm cột soft delete vào bảng NhanViens
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('NhanViens') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE NhanViens ADD IsDeleted BIT NOT NULL DEFAULT 0;
    ALTER TABLE NhanViens ADD NgayXoa DATETIME2 NULL;
    ALTER TABLE NhanViens ADD NguoiXoa NVARCHAR(100) NULL;
    PRINT 'NhanViens: soft delete columns added';
END

-- 2. Thêm cột soft delete vào bảng KhachHangs
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('KhachHangs') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE KhachHangs ADD IsDeleted BIT NOT NULL DEFAULT 0;
    ALTER TABLE KhachHangs ADD NgayXoa DATETIME2 NULL;
    ALTER TABLE KhachHangs ADD NguoiXoa NVARCHAR(100) NULL;
    PRINT 'KhachHangs: soft delete columns added';
END

-- 3. Thêm cột soft delete vào bảng DanhMucs
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DanhMucs') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE DanhMucs ADD IsDeleted BIT NOT NULL DEFAULT 0;
    ALTER TABLE DanhMucs ADD NgayXoa DATETIME2 NULL;
    ALTER TABLE DanhMucs ADD NguoiXoa NVARCHAR(100) NULL;
    PRINT 'DanhMucs: soft delete columns added';
END

-- 4. Thêm cột soft delete vào bảng ChiNhanhs
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ChiNhanhs') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE ChiNhanhs ADD IsDeleted BIT NOT NULL DEFAULT 0;
    ALTER TABLE ChiNhanhs ADD NgayXoa DATETIME2 NULL;
    ALTER TABLE ChiNhanhs ADD NguoiXoa NVARCHAR(100) NULL;
    PRINT 'ChiNhanhs: soft delete columns added';
END

-- 5. Bảng CauHinhHeThong (Cấu hình Super Admin)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CauHinhHeThangs')
BEGIN
    CREATE TABLE CauHinhHeThangs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        NhomCauHinh NVARCHAR(50) NOT NULL,       -- 'General', 'Payment', 'Email', 'System'
        MaKey NVARCHAR(100) NOT NULL,              -- 'SiteLogo', 'PrimaryColor', 'SePayWebhook'
        GiaTri NVARCHAR(MAX) NOT NULL DEFAULT '',  -- Giá trị (URL, hex, JSON)
        MoTa NVARCHAR(500) NULL,
        NgayCapNhat DATETIME2 NOT NULL DEFAULT GETDATE(),
        NguoiCapNhat NVARCHAR(100) NULL,
        CONSTRAINT UQ_CauHinh_NhomKey UNIQUE (NhomCauHinh, MaKey)
    );

    -- Seed dữ liệu mặc định
    INSERT INTO CauHinhHeThangs (NhomCauHinh, MaKey, GiaTri, MoTa) VALUES
    ('General', 'SiteName', 'POS36', 'Tên hệ thống hiển thị'),
    ('General', 'SiteLogo', '', 'URL logo hệ thống (để trống dùng text)'),
    ('General', 'Slogan', 'Giải pháp quản lý nhà hàng thông minh', 'Slogan hiển thị trên trang chủ'),
    ('General', 'SupportEmail', 'support@pos36.vn', 'Email hỗ trợ khách hàng'),
    ('General', 'SupportPhone', '', 'Số điện thoại hỗ trợ'),
    ('General', 'PrimaryColor', '#f59e0b', 'Màu chủ đề chính (hex)'),
    ('General', 'TrialDays', '7', 'Số ngày dùng thử miễn phí'),
    ('Payment', 'SePayWebhookSecret', '', 'SePay webhook secret key'),
    ('Payment', 'BankCode', 'MBBank', 'Mã ngân hàng nhận thanh toán'),
    ('Payment', 'BankAccountNo', '', 'Số tài khoản ngân hàng'),
    ('Payment', 'BankAccountName', '', 'Tên chủ tài khoản'),
    ('Email', 'EmailJsServiceId', '', 'EmailJS Service ID'),
    ('Email', 'EmailJsTemplateId', '', 'EmailJS Template ID'),
    ('Email', 'EmailJsPublicKey', '', 'EmailJS Public Key'),
    ('Email', 'SmtpHost', 'smtp.gmail.com', 'SMTP Server host'),
    ('Email', 'SmtpPort', '587', 'SMTP Port'),
    ('Email', 'SmtpUser', '', 'SMTP Username/Email'),
    ('Email', 'SmtpPassword', '', 'SMTP Password (mã hóa nên lưu qua Secret)'),
    ('Email', 'SmtpFromName', 'POS36 System', 'Tên hiển thị khi gửi email');

    PRINT 'CauHinhHeThangs: table created with default data';
END

-- 6. Bảng NhatKyHeThong (System Audit Log)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NhatKyHeThangs')
BEGIN
    CREATE TABLE NhatKyHeThangs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        HanhDong NVARCHAR(50) NOT NULL,           -- 'Tao', 'Sua', 'Xoa', 'DangNhap', 'CauHinh', 'KhoaCuaHang'
        MoTa NVARCHAR(500) NOT NULL,               -- 'Xóa cửa hàng #5 - Quán Café ABC'
        UrlLienQuan NVARCHAR(500) NULL,            -- '/super-admin/stores?id=5'
        IpAddress NVARCHAR(50) NULL,
        NguoiThucHien NVARCHAR(100) NULL,
        ThoiGian DATETIME2 NOT NULL DEFAULT GETDATE(),
        ChiTietJson NVARCHAR(MAX) NULL              -- JSON snapshot trước/sau
    );

    -- Index để query nhanh
    CREATE INDEX IX_NhatKy_ThoiGian ON NhatKyHeThangs (ThoiGian DESC);
    CREATE INDEX IX_NhatKy_HanhDong ON NhatKyHeThangs (HanhDong);

    PRINT 'NhatKyHeThangs: table created';
END

PRINT '=== Phase 6 Migration completed successfully ===';
