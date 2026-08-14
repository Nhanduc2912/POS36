using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS36.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameCauHinhHeThangsToCauHinhHeThongs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Bảng đã được đổi tên thủ công trong SQL Server thành CauHinhHeThongs
            // Migration này chỉ dùng để cập nhật EF Model Snapshot cho đồng bộ
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
