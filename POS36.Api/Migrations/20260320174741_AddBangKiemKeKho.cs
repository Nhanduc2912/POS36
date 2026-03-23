using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS36.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBangKiemKeKho : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PhieuKiemKes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaChungTu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CuaHangId = table.Column<int>(type: "int", nullable: false),
                    ChiNhanhId = table.Column<int>(type: "int", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhieuKiemKes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietKiemKes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhieuKiemKeId = table.Column<int>(type: "int", nullable: false),
                    SanPhamId = table.Column<int>(type: "int", nullable: false),
                    TonKhoHienTai = table.Column<int>(type: "int", nullable: false),
                    SoLuongKiemKe = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietKiemKes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietKiemKes_PhieuKiemKes_PhieuKiemKeId",
                        column: x => x.PhieuKiemKeId,
                        principalTable: "PhieuKiemKes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietKiemKes_SanPhams_SanPhamId",
                        column: x => x.SanPhamId,
                        principalTable: "SanPhams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietKiemKes_PhieuKiemKeId",
                table: "ChiTietKiemKes",
                column: "PhieuKiemKeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietKiemKes_SanPhamId",
                table: "ChiTietKiemKes",
                column: "SanPhamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietKiemKes");

            migrationBuilder.DropTable(
                name: "PhieuKiemKes");
        }
    }
}
