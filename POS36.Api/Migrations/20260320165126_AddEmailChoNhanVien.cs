using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS36.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailChoNhanVien : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "NhanViens",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "NhanViens");
        }
    }
}
