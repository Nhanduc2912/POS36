using Microsoft.EntityFrameworkCore;
using POS36.Api.Models;
using System.Security.Claims;

namespace POS36.Api.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor? httpContextAccessor = null)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Helper: lấy CuaHangId từ JWT. Trả về 0 nếu là SuperAdmin hoặc chưa auth.
        private int? GetCurrentCuaHangId()
        {
            var user = _httpContextAccessor?.HttpContext?.User;
            if (user == null || !user.Identity!.IsAuthenticated) return null;

            var role = user.FindFirst(ClaimTypes.Role)?.Value ?? user.FindFirst("VaiTro")?.Value;
            if (role == "SuperAdmin") return null; // SuperAdmin thấy tất cả

            var claim = user.FindFirst("CuaHangId")?.Value;
            return int.TryParse(claim, out int id) ? id : null;
        }

        // ===== PHÂN HỆ 1: Hệ thống & Nhân sự =====
        public DbSet<CuaHang> CuaHangs { get; set; }
        public DbSet<ChiNhanh> ChiNhanhs { get; set; }
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<NhanVien> NhanViens { get; set; }
        public DbSet<KhachHang> KhachHangs { get; set; }
        public DbSet<OtpRequest> OtpRequests { get; set; }

        // ===== PHÂN HỆ 2: Bán hàng =====
        public DbSet<KhuVuc> KhuVucs { get; set; }
        public DbSet<Ban> Bans { get; set; }
        public DbSet<DanhMuc> DanhMucs { get; set; }
        public DbSet<SanPham> SanPhams { get; set; }

        // ===== PHÂN HỆ 3: Giao dịch =====
        public DbSet<HoaDon> HoaDons { get; set; }
        public DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; }
        public DbSet<ThanhToan> ThanhToans { get; set; }

        // ===== PHÂN HỆ 4: Kho =====
        public DbSet<TonKho> TonKhos { get; set; }
        public DbSet<PhieuNhap> PhieuNhaps { get; set; }
        public DbSet<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; }
        public DbSet<LichSuKho> LichSuKhos { get; set; }

        // ===== PHÂN HỆ 5: Kiểm kê =====
        public DbSet<PhieuKiemKe> PhieuKiemKes { get; set; }
        public DbSet<ChiTietKiemKe> ChiTietKiemKes { get; set; }

        // ===== PHÂN HỆ 6: Sổ Quỹ =====
        public DbSet<PhieuThuChi> PhieuThuChis { get; set; }
        public DbSet<ThietLap> ThietLaps { get; set; }

        // ===== PHÂN HỆ 7: SaaS (Global — không filter theo tenant) =====
        public DbSet<GoiDichVu> GoiDichVus { get; set; }
        public DbSet<LichSuDangKy> LichSuDangKys { get; set; }
        public DbSet<LuotTruyCap> LuotTruyCaps { get; set; }
        public DbSet<ThongBaoHeThong> ThongBaoHeThongs { get; set; }

        // ===== Phase 6: Config & Audit Log (Global) =====
        public DbSet<CauHinhHeThong> CauHinhHeThangs { get; set; }
        public DbSet<NhatKyHeThong> NhatKyHeThangs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ====================================================
            // GLOBAL QUERY FILTERS — Tự động lọc theo CuaHangId
            // SuperAdmin (GetCurrentCuaHangId() == null) → thấy TẤT CẢ
            // Nhân viên/Chủ quán → chỉ thấy dữ liệu quán mình
            // ====================================================

            modelBuilder.Entity<ChiNhanh>().HasQueryFilter(e =>
                GetCurrentCuaHangId() == null || (e.CuaHangId == GetCurrentCuaHangId() && !e.IsDeleted));

            modelBuilder.Entity<TaiKhoan>().HasQueryFilter(e =>
                GetCurrentCuaHangId() == null || e.CuaHangId == GetCurrentCuaHangId());

            modelBuilder.Entity<NhanVien>().HasQueryFilter(e =>
                GetCurrentCuaHangId() == null || (e.CuaHangId == GetCurrentCuaHangId() && !e.IsDeleted));

            modelBuilder.Entity<KhachHang>().HasQueryFilter(e =>
                GetCurrentCuaHangId() == null || (e.CuaHangId == GetCurrentCuaHangId() && !e.IsDeleted));

            modelBuilder.Entity<KhuVuc>().HasQueryFilter(e =>
                GetCurrentCuaHangId() == null || e.CuaHangId == GetCurrentCuaHangId());

            modelBuilder.Entity<Ban>().HasQueryFilter(e =>
                GetCurrentCuaHangId() == null || e.CuaHangId == GetCurrentCuaHangId());

            modelBuilder.Entity<DanhMuc>().HasQueryFilter(e =>
                GetCurrentCuaHangId() == null || (e.CuaHangId == GetCurrentCuaHangId() && !e.IsDeleted));

            modelBuilder.Entity<SanPham>().HasQueryFilter(e =>
                GetCurrentCuaHangId() == null || e.CuaHangId == GetCurrentCuaHangId());

            modelBuilder.Entity<HoaDon>().HasQueryFilter(e =>
                GetCurrentCuaHangId() == null || e.CuaHangId == GetCurrentCuaHangId());

            // ChiTietHoaDon: filter gián tiếp qua HoaDon (Include sẽ tự lọc)

            modelBuilder.Entity<TonKho>().HasQueryFilter(e =>
                GetCurrentCuaHangId() == null || e.ChiNhanhId != 0); // Filter qua ChiNhanh

            modelBuilder.Entity<PhieuNhap>().HasQueryFilter(e =>
                GetCurrentCuaHangId() == null || e.CuaHangId == GetCurrentCuaHangId());

            modelBuilder.Entity<PhieuThuChi>().HasQueryFilter(e =>
                GetCurrentCuaHangId() == null || e.CuaHangId == GetCurrentCuaHangId());

            modelBuilder.Entity<ThietLap>().HasQueryFilter(e =>
                GetCurrentCuaHangId() == null || e.CuaHangId == GetCurrentCuaHangId());

            // ====================================================
            // CHỐNG LỖI CASCADE DELETE (Giữ nguyên)
            // ====================================================
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}