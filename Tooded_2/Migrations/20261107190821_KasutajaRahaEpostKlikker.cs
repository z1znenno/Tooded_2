using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tooded_2.Migrations
{
    /// <inheritdoc />
    public partial class KasutajaRahaEpostKlikker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AbilisteArv",
                table: "User",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "User",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EpostiParool",
                table: "User",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Raha",
                table: "User",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TooriistaTase",
                table: "User",
                type: "integer",
                nullable: false,
                defaultValue: 1); // tase 1, muidu klikk annaks 0 €

            // Vana kood lubas sama nimega kasutajaid – nimetame duplikaadid ümber
            // (nt "oleg" -> "oleg_5"), et unikaalset indeksit saaks luua.
            migrationBuilder.Sql(@"
                UPDATE ""User"" u
                SET ""Username"" = u.""Username"" || '_' || u.""Id""
                WHERE EXISTS (
                    SELECT 1 FROM ""User"" o
                    WHERE o.""Username"" = u.""Username"" AND o.""Id"" < u.""Id"");");

            migrationBuilder.CreateIndex(
                name: "IX_User_Username",
                table: "User",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_Username",
                table: "User");

            migrationBuilder.DropColumn(
                name: "AbilisteArv",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "User");

            migrationBuilder.DropColumn(
                name: "EpostiParool",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Raha",
                table: "User");

            migrationBuilder.DropColumn(
                name: "TooriistaTase",
                table: "User");
        }
    }
}
