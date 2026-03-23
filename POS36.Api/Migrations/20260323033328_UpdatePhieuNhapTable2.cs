using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS36.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePhieuNhapTable2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CuaHangId",
                table: "PhieuNhaps",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MaChungTu",
                table: "PhieuNhaps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TrangThai",
                table: "PhieuNhaps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CuaHangId",
                table: "PhieuNhaps");

            migrationBuilder.DropColumn(
                name: "MaChungTu",
                table: "PhieuNhaps");

            migrationBuilder.DropColumn(
                name: "TrangThai",
                table: "PhieuNhaps");
        }
    }
}
