using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS36.Api.Migrations
{
    /// <inheritdoc />
    public partial class ThemChiTietKhachHang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DiemTichLuy",
                table: "KhachHangs",
                newName: "TongDiemTichLuy");

            migrationBuilder.AddColumn<string>(
                name: "DiaChi",
                table: "KhachHangs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiemHienTai",
                table: "KhachHangs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "KhachHangs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GhiChu",
                table: "KhachHangs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgaySinh",
                table: "KhachHangs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayTao",
                table: "KhachHangs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiaChi",
                table: "KhachHangs");

            migrationBuilder.DropColumn(
                name: "DiemHienTai",
                table: "KhachHangs");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "KhachHangs");

            migrationBuilder.DropColumn(
                name: "GhiChu",
                table: "KhachHangs");

            migrationBuilder.DropColumn(
                name: "NgaySinh",
                table: "KhachHangs");

            migrationBuilder.DropColumn(
                name: "NgayTao",
                table: "KhachHangs");

            migrationBuilder.RenameColumn(
                name: "TongDiemTichLuy",
                table: "KhachHangs",
                newName: "DiemTichLuy");
        }
    }
}
