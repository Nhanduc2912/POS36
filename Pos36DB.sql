-- Kiểm tra xem database đã tồn tại chưa, nếu chưa thì tự động tạo mới
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'POS36_Db')
BEGIN
    CREATE DATABASE [POS36_Db]
END
GO

-- Chuyển sang sử dụng database vừa tạo
USE [POS36_Db]
GO

/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 3/14/2026 1:49:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Bans]    Script Date: 3/14/2026 1:49:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Bans](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CuaHangId] [int] NOT NULL,
	[KhuVucId] [int] NOT NULL,
	[TenBan] [nvarchar](max) NOT NULL,
	[TrangThai] [nvarchar](max) NOT NULL,
	[MaBan] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_Bans] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ChiNhanhs]    Script Date: 3/14/2026 1:49:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ChiNhanhs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CuaHangId] [int] NOT NULL,
	[TenChiNhanh] [nvarchar](max) NOT NULL,
	[DiaChi] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_ChiNhanhs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ChiTietHoaDons]    Script Date: 3/14/2026 1:49:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ChiTietHoaDons](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[HoaDonId] [int] NOT NULL,
	[SanPhamId] [int] NOT NULL,
	[SoLuong] [int] NOT NULL,
	[DonGia] [decimal](18, 2) NOT NULL,
	[GhiChu] [nvarchar](max) NOT NULL,
	[TrangThaiMon] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_ChiTietHoaDons] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ChiTietPhieuNhaps]    Script Date: 3/14/2026 1:49:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ChiTietPhieuNhaps](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PhieuNhapId] [int] NOT NULL,
	[SanPhamId] [int] NOT NULL,
	[SoLuong] [int] NOT NULL,
	[DonGiaNhap] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_ChiTietPhieuNhaps] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CuaHangs]    Script Date: 3/14/2026 1:49:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CuaHangs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TenCuaHang] [nvarchar](max) NOT NULL,
	[SoDienThoai] [nvarchar](max) NOT NULL,
	[NgayDangKy] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_CuaHangs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DanhMucs]    Script Date: 3/14/2026 1:49:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DanhMucs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CuaHangId] [int] NOT NULL,
	[TenDanhMuc] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_DanhMucs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HoaDons]    Script Date: 3/14/2026 1:49:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HoaDons](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CuaHangId] [int] NOT NULL,
	[ChiNhanhId] [int] NOT NULL,
	[BanId] [int] NOT NULL,
	[NhanVienId] [int] NULL,
	[NgayTao] [datetime2](7) NOT NULL,
	[NgayThanhToan] [datetime2](7) NULL,
	[TongTien] [decimal](18, 2) NOT NULL,
	[TrangThai] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_HoaDons] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[KhachHangs]    Script Date: 3/14/2026 1:49:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[KhachHangs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CuaHangId] [int] NOT NULL,
	[TenKhachHang] [nvarchar](max) NOT NULL,
	[SoDienThoai] [nvarchar](max) NOT NULL,
	[DiemTichLuy] [int] NOT NULL,
 CONSTRAINT [PK_KhachHangs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[KhuVucs]    Script Date: 3/14/2026 1:49:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[KhuVucs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CuaHangId] [int] NOT NULL,
	[ChiNhanhId] [int] NOT NULL,
	[TenKhuVuc] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_KhuVucs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LichSuKhos]    Script Date: 3/14/2026 1:49:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LichSuKhos](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ChiNhanhId] [int] NOT NULL,
	[SanPhamId] [int] NOT NULL,
	[LoaiGiaoDich] [nvarchar](max) NOT NULL,
	[SoLuong] [int] NOT NULL,
	[NgayThucHien] [datetime2](7) NOT NULL,
	[GhiChu] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_LichSuKhos] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NhanViens]    Script Date: 3/14/2026 1:49:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NhanViens](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CuaHangId] [int] NOT NULL,
	[ChiNhanhId] [int] NULL,
	[MaNhanVien] [nvarchar](max) NOT NULL,
	[TenNhanVien] [nvarchar](max) NOT NULL,
	[SoDienThoai] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_NhanViens] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PhieuNhaps]    Script Date: 3/14/2026 1:49:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PhieuNhaps](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ChiNhanhId] [int] NOT NULL,
	[TaiKhoanId] [int] NOT NULL,
	[NgayNhap] [datetime2](7) NOT NULL,
	[GhiChu] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_PhieuNhaps] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SanPhams]    Script Date: 3/14/2026 1:49:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SanPhams](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CuaHangId] [int] NOT NULL,
	[DanhMucId] [int] NOT NULL,
	[TenSanPham] [nvarchar](max) NOT NULL,
	[GiaBan] [decimal](18, 2) NOT NULL,
	[TrangThai] [bit] NOT NULL,
 CONSTRAINT [PK_SanPhams] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TaiKhoans]    Script Date: 3/14/2026 1:49:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TaiKhoans](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CuaHangId] [int] NOT NULL,
	[ChiNhanhId] [int] NULL,
	[TenDangNhap] [nvarchar](max) NOT NULL,
	[MatKhauHash] [nvarchar](max) NOT NULL,
	[VaiTro] [nvarchar](max) NOT NULL,
	[NhanVienId] [int] NULL,
 CONSTRAINT [PK_TaiKhoans] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ThanhToans]    Script Date: 3/14/2026 1:49:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ThanhToans](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[HoaDonId] [int] NOT NULL,
	[PhuongThuc] [nvarchar](max) NOT NULL,
	[SoTien] [decimal](18, 2) NOT NULL,
	[NgayThanhToan] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_ThanhToans] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TonKhos]    Script Date: 3/14/2026 1:49:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TonKhos](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ChiNhanhId] [int] NOT NULL,
	[SanPhamId] [int] NOT NULL,
	[SoLuong] [int] NOT NULL,
 CONSTRAINT [PK_TonKhos] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

-- KHÓA NGOẠI VÀ RÀNG BUỘC
ALTER TABLE [dbo].[Bans] ADD  DEFAULT (N'') FOR [MaBan]
GO
ALTER TABLE [dbo].[Bans]  WITH CHECK ADD  CONSTRAINT [FK_Bans_KhuVucs_KhuVucId] FOREIGN KEY([KhuVucId])
REFERENCES [dbo].[KhuVucs] ([Id])
GO
ALTER TABLE [dbo].[Bans] CHECK CONSTRAINT [FK_Bans_KhuVucs_KhuVucId]
GO
ALTER TABLE [dbo].[ChiNhanhs]  WITH CHECK ADD  CONSTRAINT [FK_ChiNhanhs_CuaHangs_CuaHangId] FOREIGN KEY([CuaHangId])
REFERENCES [dbo].[CuaHangs] ([Id])
GO
ALTER TABLE [dbo].[ChiNhanhs] CHECK CONSTRAINT [FK_ChiNhanhs_CuaHangs_CuaHangId]
GO
ALTER TABLE [dbo].[ChiTietHoaDons]  WITH CHECK ADD  CONSTRAINT [FK_ChiTietHoaDons_HoaDons_HoaDonId] FOREIGN KEY([HoaDonId])
REFERENCES [dbo].[HoaDons] ([Id])
GO
ALTER TABLE [dbo].[ChiTietHoaDons] CHECK CONSTRAINT [FK_ChiTietHoaDons_HoaDons_HoaDonId]
GO
ALTER TABLE [dbo].[ChiTietHoaDons]  WITH CHECK ADD  CONSTRAINT [FK_ChiTietHoaDons_SanPhams_SanPhamId] FOREIGN KEY([SanPhamId])
REFERENCES [dbo].[SanPhams] ([Id])
GO
ALTER TABLE [dbo].[ChiTietHoaDons] CHECK CONSTRAINT [FK_ChiTietHoaDons_SanPhams_SanPhamId]
GO
ALTER TABLE [dbo].[ChiTietPhieuNhaps]  WITH CHECK ADD  CONSTRAINT [FK_ChiTietPhieuNhaps_PhieuNhaps_PhieuNhapId] FOREIGN KEY([PhieuNhapId])
REFERENCES [dbo].[PhieuNhaps] ([Id])
GO
ALTER TABLE [dbo].[ChiTietPhieuNhaps] CHECK CONSTRAINT [FK_ChiTietPhieuNhaps_PhieuNhaps_PhieuNhapId]
GO
ALTER TABLE [dbo].[ChiTietPhieuNhaps]  WITH CHECK ADD  CONSTRAINT [FK_ChiTietPhieuNhaps_SanPhams_SanPhamId] FOREIGN KEY([SanPhamId])
REFERENCES [dbo].[SanPhams] ([Id])
GO
ALTER TABLE [dbo].[ChiTietPhieuNhaps] CHECK CONSTRAINT [FK_ChiTietPhieuNhaps_SanPhams_SanPhamId]
GO
ALTER TABLE [dbo].[HoaDons]  WITH CHECK ADD  CONSTRAINT [FK_HoaDons_Bans_BanId] FOREIGN KEY([BanId])
REFERENCES [dbo].[Bans] ([Id])
GO
ALTER TABLE [dbo].[HoaDons] CHECK CONSTRAINT [FK_HoaDons_Bans_BanId]
GO
ALTER TABLE [dbo].[KhuVucs]  WITH CHECK ADD  CONSTRAINT [FK_KhuVucs_ChiNhanhs_ChiNhanhId] FOREIGN KEY([ChiNhanhId])
REFERENCES [dbo].[ChiNhanhs] ([Id])
GO
ALTER TABLE [dbo].[KhuVucs] CHECK CONSTRAINT [FK_KhuVucs_ChiNhanhs_ChiNhanhId]
GO
ALTER TABLE [dbo].[LichSuKhos]  WITH CHECK ADD  CONSTRAINT [FK_LichSuKhos_ChiNhanhs_ChiNhanhId] FOREIGN KEY([ChiNhanhId])
REFERENCES [dbo].[ChiNhanhs] ([Id])
GO
ALTER TABLE [dbo].[LichSuKhos] CHECK CONSTRAINT [FK_LichSuKhos_ChiNhanhs_ChiNhanhId]
GO
ALTER TABLE [dbo].[LichSuKhos]  WITH CHECK ADD  CONSTRAINT [FK_LichSuKhos_SanPhams_SanPhamId] FOREIGN KEY([SanPhamId])
REFERENCES [dbo].[SanPhams] ([Id])
GO
ALTER TABLE [dbo].[LichSuKhos] CHECK CONSTRAINT [FK_LichSuKhos_SanPhams_SanPhamId]
GO
ALTER TABLE [dbo].[NhanViens]  WITH CHECK ADD  CONSTRAINT [FK_NhanViens_ChiNhanhs_ChiNhanhId] FOREIGN KEY([ChiNhanhId])
REFERENCES [dbo].[ChiNhanhs] ([Id])
GO
ALTER TABLE [dbo].[NhanViens] CHECK CONSTRAINT [FK_NhanViens_ChiNhanhs_ChiNhanhId]
GO
ALTER TABLE [dbo].[SanPhams]  WITH CHECK ADD  CONSTRAINT [FK_SanPhams_DanhMucs_DanhMucId] FOREIGN KEY([DanhMucId])
REFERENCES [dbo].[DanhMucs] ([Id])
GO
ALTER TABLE [dbo].[SanPhams] CHECK CONSTRAINT [FK_SanPhams_DanhMucs_DanhMucId]
GO
ALTER TABLE [dbo].[TaiKhoans]  WITH CHECK ADD  CONSTRAINT [FK_TaiKhoans_ChiNhanhs_ChiNhanhId] FOREIGN KEY([ChiNhanhId])
REFERENCES [dbo].[ChiNhanhs] ([Id])
GO
ALTER TABLE [dbo].[TaiKhoans] CHECK CONSTRAINT [FK_TaiKhoans_ChiNhanhs_ChiNhanhId]
GO
ALTER TABLE [dbo].[TaiKhoans]  WITH CHECK ADD  CONSTRAINT [FK_TaiKhoans_CuaHangs_CuaHangId] FOREIGN KEY([CuaHangId])
REFERENCES [dbo].[CuaHangs] ([Id])
GO
ALTER TABLE [dbo].[TaiKhoans] CHECK CONSTRAINT [FK_TaiKhoans_CuaHangs_CuaHangId]
GO
ALTER TABLE [dbo].[TaiKhoans]  WITH CHECK ADD  CONSTRAINT [FK_TaiKhoans_NhanViens_NhanVienId] FOREIGN KEY([NhanVienId])
REFERENCES [dbo].[NhanViens] ([Id])
GO
ALTER TABLE [dbo].[TaiKhoans] CHECK CONSTRAINT [FK_TaiKhoans_NhanViens_NhanVienId]
GO
ALTER TABLE [dbo].[ThanhToans]  WITH CHECK ADD  CONSTRAINT [FK_ThanhToans_HoaDons_HoaDonId] FOREIGN KEY([HoaDonId])
REFERENCES [dbo].[HoaDons] ([Id])
GO
ALTER TABLE [dbo].[ThanhToans] CHECK CONSTRAINT [FK_ThanhToans_HoaDons_HoaDonId]
GO
ALTER TABLE [dbo].[TonKhos]  WITH CHECK ADD  CONSTRAINT [FK_TonKhos_ChiNhanhs_ChiNhanhId] FOREIGN KEY([ChiNhanhId])
REFERENCES [dbo].[ChiNhanhs] ([Id])
GO
ALTER TABLE [dbo].[TonKhos] CHECK CONSTRAINT [FK_TonKhos_ChiNhanhs_ChiNhanhId]
GO
ALTER TABLE [dbo].[TonKhos]  WITH CHECK ADD  CONSTRAINT [FK_TonKhos_SanPhams_SanPhamId] FOREIGN KEY([SanPhamId])
REFERENCES [dbo].[SanPhams] ([Id])
GO
ALTER TABLE [dbo].[TonKhos] CHECK CONSTRAINT [FK_TonKhos_SanPhams_SanPhamId]
GO