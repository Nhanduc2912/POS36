using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS36.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBangThietLap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ThietLaps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CuaHangId = table.Column<int>(type: "int", nullable: false),
                    MaThietLap = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DuLieu = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThietLaps", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThietLaps");
        }
    }
}
