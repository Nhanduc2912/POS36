using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS36.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchemaPOS36 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "TaiKhoans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TaiKhoans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LanDangNhapCuoi",
                table: "TaiKhoans",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayTao",
                table: "TaiKhoans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "SoDienThoai",
                table: "TaiKhoans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "NhanViens",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayXoa",
                table: "NhanViens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NguoiXoa",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "KhachHangs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayXoa",
                table: "KhachHangs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NguoiXoa",
                table: "KhachHangs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "DanhMucs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayXoa",
                table: "DanhMucs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NguoiXoa",
                table: "DanhMucs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiaChi",
                table: "CuaHangs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "CuaHangs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GhiChu",
                table: "CuaHangs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GoiDichVu",
                table: "CuaHangs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "CuaHangs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayHetHan",
                table: "CuaHangs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "TrangThai",
                table: "CuaHangs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ChiNhanhs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayXoa",
                table: "ChiNhanhs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NguoiXoa",
                table: "ChiNhanhs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CauHinhHeThangs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NhomCauHinh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GiaTri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiCapNhat = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHinhHeThangs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GoiDichVus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenGoi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaGoi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoThang = table.Column<int>(type: "int", nullable: false),
                    GiaThang = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TongGia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GioiHanHoaDon = table.Column<int>(type: "int", nullable: false),
                    GioiHanNhanVien = table.Column<int>(type: "int", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ThuTuHienThi = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoiDichVus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LuotTruyCaps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CuaHangId = table.Column<int>(type: "int", nullable: true),
                    TaiKhoanId = table.Column<int>(type: "int", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThietBi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LuotTruyCaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LuotTruyCaps_CuaHangs_CuaHangId",
                        column: x => x.CuaHangId,
                        principalTable: "CuaHangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NhatKyHeThangs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HanhDong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UrlLienQuan = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NguoiThucHien = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChiTietJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKyHeThangs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ThongBaoHeThongs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CuaHangId = table.Column<int>(type: "int", nullable: true),
                    TieuDe = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoaiThongBao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DaDoc = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBaoHeThongs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThongBaoHeThongs_CuaHangs_CuaHangId",
                        column: x => x.CuaHangId,
                        principalTable: "CuaHangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LichSuDangKys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CuaHangId = table.Column<int>(type: "int", nullable: false),
                    GoiDichVuId = table.Column<int>(type: "int", nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoTienThanhToan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhuongThucThanhToan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaGiaoDich = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayThanhToan = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NguoiDuyet = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuDangKys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LichSuDangKys_CuaHangs_CuaHangId",
                        column: x => x.CuaHangId,
                        principalTable: "CuaHangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichSuDangKys_GoiDichVus_GoiDichVuId",
                        column: x => x.GoiDichVuId,
                        principalTable: "GoiDichVus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LichSuDangKys_CuaHangId",
                table: "LichSuDangKys",
                column: "CuaHangId");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuDangKys_GoiDichVuId",
                table: "LichSuDangKys",
                column: "GoiDichVuId");

            migrationBuilder.CreateIndex(
                name: "IX_LuotTruyCaps_CuaHangId",
                table: "LuotTruyCaps",
                column: "CuaHangId");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBaoHeThongs_CuaHangId",
                table: "ThongBaoHeThongs",
                column: "CuaHangId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CauHinhHeThangs");

            migrationBuilder.DropTable(
                name: "LichSuDangKys");

            migrationBuilder.DropTable(
                name: "LuotTruyCaps");

            migrationBuilder.DropTable(
                name: "NhatKyHeThangs");

            migrationBuilder.DropTable(
                name: "ThongBaoHeThongs");

            migrationBuilder.DropTable(
                name: "GoiDichVus");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "TaiKhoans");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TaiKhoans");

            migrationBuilder.DropColumn(
                name: "LanDangNhapCuoi",
                table: "TaiKhoans");

            migrationBuilder.DropColumn(
                name: "NgayTao",
                table: "TaiKhoans");

            migrationBuilder.DropColumn(
                name: "SoDienThoai",
                table: "TaiKhoans");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "NgayXoa",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "NguoiXoa",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "KhachHangs");

            migrationBuilder.DropColumn(
                name: "NgayXoa",
                table: "KhachHangs");

            migrationBuilder.DropColumn(
                name: "NguoiXoa",
                table: "KhachHangs");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "DanhMucs");

            migrationBuilder.DropColumn(
                name: "NgayXoa",
                table: "DanhMucs");

            migrationBuilder.DropColumn(
                name: "NguoiXoa",
                table: "DanhMucs");

            migrationBuilder.DropColumn(
                name: "DiaChi",
                table: "CuaHangs");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "CuaHangs");

            migrationBuilder.DropColumn(
                name: "GhiChu",
                table: "CuaHangs");

            migrationBuilder.DropColumn(
                name: "GoiDichVu",
                table: "CuaHangs");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "CuaHangs");

            migrationBuilder.DropColumn(
                name: "NgayHetHan",
                table: "CuaHangs");

            migrationBuilder.DropColumn(
                name: "TrangThai",
                table: "CuaHangs");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ChiNhanhs");

            migrationBuilder.DropColumn(
                name: "NgayXoa",
                table: "ChiNhanhs");

            migrationBuilder.DropColumn(
                name: "NguoiXoa",
                table: "ChiNhanhs");
        }
    }
}
