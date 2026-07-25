using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS36.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEmergencyContactAndConsent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DongYXuLyDuLieu",
                table: "NhanViens",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MoiQuanHeKhanCap",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayDongY",
                table: "NhanViens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NguoiLienHeKhanCap",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SdtKhanCap",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DongYXuLyDuLieu",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "MoiQuanHeKhanCap",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "NgayDongY",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "NguoiLienHeKhanCap",
                table: "NhanViens");

            migrationBuilder.DropColumn(
                name: "SdtKhanCap",
                table: "NhanViens");
        }
    }
}
