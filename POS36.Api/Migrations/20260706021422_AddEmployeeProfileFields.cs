using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS36.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cccd",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DiaChiTamTru",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DiaChiThuongTru",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GioiTinh",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayCapCccd",
                table: "NhanViens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgaySinh",
                table: "NhanViens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayVaoLam",
                table: "NhanViens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoiCapCccd",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cccd",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "DiaChiTamTru",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "DiaChiThuongTru",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "GioiTinh",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "NgayCapCccd",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "NgaySinh",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "NgayVaoLam",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "NoiCapCccd",
                table: "NhanViens");
        }
    }
}
