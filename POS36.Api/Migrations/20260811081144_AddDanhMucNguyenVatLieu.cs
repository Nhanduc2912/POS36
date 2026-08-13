using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS36.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDanhMucNguyenVatLieu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NguyenVatLieus_DanhMucs_DanhMucId",
                table: "NguyenVatLieus");

            migrationBuilder.RenameColumn(
                name: "DanhMucId",
                table: "NguyenVatLieus",
                newName: "DanhMucNguyenVatLieuId");

            migrationBuilder.Sql("UPDATE NguyenVatLieus SET DanhMucNguyenVatLieuId = NULL;");

            migrationBuilder.RenameIndex(
                name: "IX_NguyenVatLieus_DanhMucId",
                table: "NguyenVatLieus",
                newName: "IX_NguyenVatLieus_DanhMucNguyenVatLieuId");

            migrationBuilder.CreateTable(
                name: "DanhMucNguyenVatLieus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CuaHangId = table.Column<int>(type: "int", nullable: false),
                    TenDanhMuc = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HinhAnh = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMucNguyenVatLieus", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_NguyenVatLieus_DanhMucNguyenVatLieus_DanhMucNguyenVatLieuId",
                table: "NguyenVatLieus",
                column: "DanhMucNguyenVatLieuId",
                principalTable: "DanhMucNguyenVatLieus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NguyenVatLieus_DanhMucNguyenVatLieus_DanhMucNguyenVatLieuId",
                table: "NguyenVatLieus");

            migrationBuilder.DropTable(
                name: "DanhMucNguyenVatLieus");

            migrationBuilder.RenameColumn(
                name: "DanhMucNguyenVatLieuId",
                table: "NguyenVatLieus",
                newName: "DanhMucId");

            migrationBuilder.RenameIndex(
                name: "IX_NguyenVatLieus_DanhMucNguyenVatLieuId",
                table: "NguyenVatLieus",
                newName: "IX_NguyenVatLieus_DanhMucId");

            migrationBuilder.AddForeignKey(
                name: "FK_NguyenVatLieus_DanhMucs_DanhMucId",
                table: "NguyenVatLieus",
                column: "DanhMucId",
                principalTable: "DanhMucs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
