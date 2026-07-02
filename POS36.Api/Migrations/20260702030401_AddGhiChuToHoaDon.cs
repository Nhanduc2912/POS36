using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS36.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddGhiChuToHoaDon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NhatKyHoatDongs_CuaHangId",
                table: "NhatKyHoatDongs");

            migrationBuilder.AddColumn<string>(
                name: "GhiChu",
                table: "HoaDons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyHoatDongs_CuaHangId_ThoiGian",
                table: "NhatKyHoatDongs",
                columns: new[] { "CuaHangId", "ThoiGian" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NhatKyHoatDongs_CuaHangId_ThoiGian",
                table: "NhatKyHoatDongs");

            migrationBuilder.DropColumn(
                name: "GhiChu",
                table: "HoaDons");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyHoatDongs_CuaHangId",
                table: "NhatKyHoatDongs",
                column: "CuaHangId");
        }
    }
}
