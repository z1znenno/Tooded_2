using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tooded_2.Migrations
{
    /// <inheritdoc />
    public partial class RemoveKategooriaKirjeldus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "kirjeldus",
                table: "Kategooriatabel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "kirjeldus",
                table: "Kategooriatabel",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
