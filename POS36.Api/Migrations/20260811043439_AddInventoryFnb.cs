using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS36.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryFnb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // XÓA DỮ LIỆU KHO CŨ ĐỂ CHUYỂN ĐỔI SANG MÔ HÌNH F&B THEO YÊU CẦU
            migrationBuilder.Sql("DELETE FROM TonKhos;");
            migrationBuilder.Sql("DELETE FROM LichSuKhos;");
            migrationBuilder.Sql("DELETE FROM ChiTietPhieuNhaps;");
            migrationBuilder.Sql("DELETE FROM PhieuNhaps;");
            migrationBuilder.Sql("DELETE FROM ChiTietKiemKes;");
            migrationBuilder.Sql("DELETE FROM PhieuKiemKes;");

            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietKiemKes_SanPhams_SanPhamId",
                table: "ChiTietKiemKes");

            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietPhieuNhaps_SanPhams_SanPhamId",
                table: "ChiTietPhieuNhaps");

            migrationBuilder.DropForeignKey(
                name: "FK_LichSuKhos_SanPhams_SanPhamId",
                table: "LichSuKhos");

            migrationBuilder.DropForeignKey(
                name: "FK_TonKhos_SanPhams_SanPhamId",
                table: "TonKhos");

            migrationBuilder.RenameColumn(
                name: "SanPhamId",
                table: "TonKhos",
                newName: "NguyenVatLieuId");

            migrationBuilder.RenameIndex(
                name: "IX_TonKhos_SanPhamId",
                table: "TonKhos",
                newName: "IX_TonKhos_NguyenVatLieuId");

            migrationBuilder.RenameColumn(
                name: "SanPhamId",
                table: "LichSuKhos",
                newName: "NguyenVatLieuId");

            migrationBuilder.RenameIndex(
                name: "IX_LichSuKhos_SanPhamId",
                table: "LichSuKhos",
                newName: "IX_LichSuKhos_NguyenVatLieuId");

            migrationBuilder.RenameColumn(
                name: "SanPhamId",
                table: "ChiTietPhieuNhaps",
                newName: "NguyenVatLieuId");

            migrationBuilder.RenameIndex(
                name: "IX_ChiTietPhieuNhaps_SanPhamId",
                table: "ChiTietPhieuNhaps",
                newName: "IX_ChiTietPhieuNhaps_NguyenVatLieuId");

            migrationBuilder.RenameColumn(
                name: "SanPhamId",
                table: "ChiTietKiemKes",
                newName: "NguyenVatLieuId");

            migrationBuilder.RenameIndex(
                name: "IX_ChiTietKiemKes_SanPhamId",
                table: "ChiTietKiemKes",
                newName: "IX_ChiTietKiemKes_NguyenVatLieuId");

            migrationBuilder.AlterColumn<decimal>(
                name: "SoLuong",
                table: "TonKhos",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayHetHan",
                table: "TonKhos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SoLuong",
                table: "LichSuKhos",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayHetHan",
                table: "LichSuKhos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SoLuong",
                table: "ChiTietPhieuNhaps",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayHetHan",
                table: "ChiTietPhieuNhaps",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TonKhoHienTai",
                table: "ChiTietKiemKes",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "SoLuongKiemKe",
                table: "ChiTietKiemKes",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayHetHan",
                table: "ChiTietKiemKes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NguyenVatLieus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CuaHangId = table.Column<int>(type: "int", nullable: false),
                    DanhMucId = table.Column<int>(type: "int", nullable: true),
                    TenNguyenVatLieu = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DonViTinh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NguongCanhBao = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoNgayCanhBaoHetHan = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguyenVatLieus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NguyenVatLieus_DanhMucs_DanhMucId",
                        column: x => x.DanhMucId,
                        principalTable: "DanhMucs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DinhLuongs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SanPhamId = table.Column<int>(type: "int", nullable: false),
                    NguyenVatLieuId = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DinhLuongs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DinhLuongs_NguyenVatLieus_NguyenVatLieuId",
                        column: x => x.NguyenVatLieuId,
                        principalTable: "NguyenVatLieus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DinhLuongs_SanPhams_SanPhamId",
                        column: x => x.SanPhamId,
                        principalTable: "SanPhams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DinhLuongs_NguyenVatLieuId",
                table: "DinhLuongs",
                column: "NguyenVatLieuId");

            migrationBuilder.CreateIndex(
                name: "IX_DinhLuongs_SanPhamId",
                table: "DinhLuongs",
                column: "SanPhamId");

            migrationBuilder.CreateIndex(
                name: "IX_NguyenVatLieus_DanhMucId",
                table: "NguyenVatLieus",
                column: "DanhMucId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietKiemKes_NguyenVatLieus_NguyenVatLieuId",
                table: "ChiTietKiemKes",
                column: "NguyenVatLieuId",
                principalTable: "NguyenVatLieus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietPhieuNhaps_NguyenVatLieus_NguyenVatLieuId",
                table: "ChiTietPhieuNhaps",
                column: "NguyenVatLieuId",
                principalTable: "NguyenVatLieus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LichSuKhos_NguyenVatLieus_NguyenVatLieuId",
                table: "LichSuKhos",
                column: "NguyenVatLieuId",
                principalTable: "NguyenVatLieus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TonKhos_NguyenVatLieus_NguyenVatLieuId",
                table: "TonKhos",
                column: "NguyenVatLieuId",
                principalTable: "NguyenVatLieus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietKiemKes_NguyenVatLieus_NguyenVatLieuId",
                table: "ChiTietKiemKes");

            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietPhieuNhaps_NguyenVatLieus_NguyenVatLieuId",
                table: "ChiTietPhieuNhaps");

            migrationBuilder.DropForeignKey(
                name: "FK_LichSuKhos_NguyenVatLieus_NguyenVatLieuId",
                table: "LichSuKhos");

            migrationBuilder.DropForeignKey(
                name: "FK_TonKhos_NguyenVatLieus_NguyenVatLieuId",
                table: "TonKhos");

            migrationBuilder.DropTable(
                name: "DinhLuongs");

            migrationBuilder.DropTable(
                name: "NguyenVatLieus");

            migrationBuilder.DropColumn(
                name: "NgayHetHan",
                table: "TonKhos");

            migrationBuilder.DropColumn(
                name: "NgayHetHan",
                table: "LichSuKhos");

            migrationBuilder.DropColumn(
                name: "NgayHetHan",
                table: "ChiTietPhieuNhaps");

            migrationBuilder.DropColumn(
                name: "NgayHetHan",
                table: "ChiTietKiemKes");

            migrationBuilder.RenameColumn(
                name: "NguyenVatLieuId",
                table: "TonKhos",
                newName: "SanPhamId");

            migrationBuilder.RenameIndex(
                name: "IX_TonKhos_NguyenVatLieuId",
                table: "TonKhos",
                newName: "IX_TonKhos_SanPhamId");

            migrationBuilder.RenameColumn(
                name: "NguyenVatLieuId",
                table: "LichSuKhos",
                newName: "SanPhamId");

            migrationBuilder.RenameIndex(
                name: "IX_LichSuKhos_NguyenVatLieuId",
                table: "LichSuKhos",
                newName: "IX_LichSuKhos_SanPhamId");

            migrationBuilder.RenameColumn(
                name: "NguyenVatLieuId",
                table: "ChiTietPhieuNhaps",
                newName: "SanPhamId");

            migrationBuilder.RenameIndex(
                name: "IX_ChiTietPhieuNhaps_NguyenVatLieuId",
                table: "ChiTietPhieuNhaps",
                newName: "IX_ChiTietPhieuNhaps_SanPhamId");

            migrationBuilder.RenameColumn(
                name: "NguyenVatLieuId",
                table: "ChiTietKiemKes",
                newName: "SanPhamId");

            migrationBuilder.RenameIndex(
                name: "IX_ChiTietKiemKes_NguyenVatLieuId",
                table: "ChiTietKiemKes",
                newName: "IX_ChiTietKiemKes_SanPhamId");

            migrationBuilder.AlterColumn<int>(
                name: "SoLuong",
                table: "TonKhos",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<int>(
                name: "SoLuong",
                table: "LichSuKhos",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<int>(
                name: "SoLuong",
                table: "ChiTietPhieuNhaps",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<int>(
                name: "TonKhoHienTai",
                table: "ChiTietKiemKes",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<int>(
                name: "SoLuongKiemKe",
                table: "ChiTietKiemKes",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietKiemKes_SanPhams_SanPhamId",
                table: "ChiTietKiemKes",
                column: "SanPhamId",
                principalTable: "SanPhams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietPhieuNhaps_SanPhams_SanPhamId",
                table: "ChiTietPhieuNhaps",
                column: "SanPhamId",
                principalTable: "SanPhams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LichSuKhos_SanPhams_SanPhamId",
                table: "LichSuKhos",
                column: "SanPhamId",
                principalTable: "SanPhams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TonKhos_SanPhams_SanPhamId",
                table: "TonKhos",
                column: "SanPhamId",
                principalTable: "SanPhams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
