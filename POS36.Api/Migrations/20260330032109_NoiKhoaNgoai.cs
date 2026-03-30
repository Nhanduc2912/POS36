using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS36.Api.Migrations
{
    /// <inheritdoc />
    public partial class NoiKhoaNgoai : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChiNhanhId",
                table: "ThietLaps",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KhachHangId",
                table: "HoaDons",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThietLaps_ChiNhanhId",
                table: "ThietLaps",
                column: "ChiNhanhId");

            migrationBuilder.CreateIndex(
                name: "IX_ThietLaps_CuaHangId",
                table: "ThietLaps",
                column: "CuaHangId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuThuChis_ChiNhanhId",
                table: "PhieuThuChis",
                column: "ChiNhanhId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuThuChis_CuaHangId",
                table: "PhieuThuChis",
                column: "CuaHangId");

            migrationBuilder.CreateIndex(
                name: "IX_KhachHangs_CuaHangId",
                table: "KhachHangs",
                column: "CuaHangId");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_KhachHangId",
                table: "HoaDons",
                column: "KhachHangId");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDons_KhachHangs_KhachHangId",
                table: "HoaDons",
                column: "KhachHangId",
                principalTable: "KhachHangs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_KhachHangs_CuaHangs_CuaHangId",
                table: "KhachHangs",
                column: "CuaHangId",
                principalTable: "CuaHangs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PhieuThuChis_ChiNhanhs_ChiNhanhId",
                table: "PhieuThuChis",
                column: "ChiNhanhId",
                principalTable: "ChiNhanhs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PhieuThuChis_CuaHangs_CuaHangId",
                table: "PhieuThuChis",
                column: "CuaHangId",
                principalTable: "CuaHangs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ThietLaps_ChiNhanhs_ChiNhanhId",
                table: "ThietLaps",
                column: "ChiNhanhId",
                principalTable: "ChiNhanhs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ThietLaps_CuaHangs_CuaHangId",
                table: "ThietLaps",
                column: "CuaHangId",
                principalTable: "CuaHangs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HoaDons_KhachHangs_KhachHangId",
                table: "HoaDons");

            migrationBuilder.DropForeignKey(
                name: "FK_KhachHangs_CuaHangs_CuaHangId",
                table: "KhachHangs");

            migrationBuilder.DropForeignKey(
                name: "FK_PhieuThuChis_ChiNhanhs_ChiNhanhId",
                table: "PhieuThuChis");

            migrationBuilder.DropForeignKey(
                name: "FK_PhieuThuChis_CuaHangs_CuaHangId",
                table: "PhieuThuChis");

            migrationBuilder.DropForeignKey(
                name: "FK_ThietLaps_ChiNhanhs_ChiNhanhId",
                table: "ThietLaps");

            migrationBuilder.DropForeignKey(
                name: "FK_ThietLaps_CuaHangs_CuaHangId",
                table: "ThietLaps");

            migrationBuilder.DropIndex(
                name: "IX_ThietLaps_ChiNhanhId",
                table: "ThietLaps");

            migrationBuilder.DropIndex(
                name: "IX_ThietLaps_CuaHangId",
                table: "ThietLaps");

            migrationBuilder.DropIndex(
                name: "IX_PhieuThuChis_ChiNhanhId",
                table: "PhieuThuChis");

            migrationBuilder.DropIndex(
                name: "IX_PhieuThuChis_CuaHangId",
                table: "PhieuThuChis");

            migrationBuilder.DropIndex(
                name: "IX_KhachHangs_CuaHangId",
                table: "KhachHangs");

            migrationBuilder.DropIndex(
                name: "IX_HoaDons_KhachHangId",
                table: "HoaDons");

            migrationBuilder.DropColumn(
                name: "ChiNhanhId",
                table: "ThietLaps");

            migrationBuilder.DropColumn(
                name: "KhachHangId",
                table: "HoaDons");
        }
    }
}
