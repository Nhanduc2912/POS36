using Microsoft.EntityFrameworkCore;
using POS36.Api.Models;

namespace POS36.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // 1. Phân hệ Hệ thống & Nhân sự
        public DbSet<CuaHang> CuaHangs { get; set; }
        public DbSet<ChiNhanh> ChiNhanhs { get; set; }
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<NhanVien> NhanViens { get; set; } // Mới
        public DbSet<KhachHang> KhachHangs { get; set; } // Mới

        // 2. Phân hệ Bán hàng
        public DbSet<KhuVuc> KhuVucs { get; set; }
        public DbSet<Ban> Bans { get; set; }
        public DbSet<DanhMuc> DanhMucs { get; set; }
        public DbSet<SanPham> SanPhams { get; set; }

        // 3. Phân hệ Giao dịch
        public DbSet<HoaDon> HoaDons { get; set; }
        public DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; }
        public DbSet<ThanhToan> ThanhToans { get; set; } // Mới

        // 4. Phân hệ Quản lý Kho PRO
        public DbSet<TonKho> TonKhos { get; set; }
        public DbSet<PhieuNhap> PhieuNhaps { get; set; }
        public DbSet<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; } // Mới
        public DbSet<LichSuKho> LichSuKhos { get; set; } // Mới

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Xử lý chống lỗi Cascade Delete (Giữ nguyên, rất quan trọng)
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict; 
            }
        }
    }
}