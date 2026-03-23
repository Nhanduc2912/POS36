using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS36.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBangThuChi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PhieuThuChis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CuaHangId = table.Column<int>(type: "int", nullable: false),
                    ChiNhanhId = table.Column<int>(type: "int", nullable: false),
                    MaChungTu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoaiPhieu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhuongThuc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NguoiNopNhan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HangMuc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GiaTri = table.Column<double>(type: "float", nullable: false),
                    NgayGiaoDich = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiTao = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhieuThuChis", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhieuThuChis");
        }
    }
}
