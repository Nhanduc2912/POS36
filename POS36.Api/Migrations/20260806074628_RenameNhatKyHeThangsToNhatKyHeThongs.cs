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
            // Bảng trong CSDL thực tế đã là NhatKyHeThongs từ trước
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
