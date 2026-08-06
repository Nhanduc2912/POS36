using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS36.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameNhatKyHeThangsToNhatKyHeThongs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_NhatKyHeThangs",
                table: "NhatKyHeThangs");

            migrationBuilder.RenameTable(
                name: "NhatKyHeThangs",
                newName: "NhatKyHeThongs");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NhatKyHeThongs",
                table: "NhatKyHeThongs",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_NhatKyHeThongs",
                table: "NhatKyHeThongs");

            migrationBuilder.RenameTable(
                name: "NhatKyHeThongs",
                newName: "NhatKyHeThangs");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NhatKyHeThangs",
                table: "NhatKyHeThangs",
                column: "Id");
        }
    }
}
