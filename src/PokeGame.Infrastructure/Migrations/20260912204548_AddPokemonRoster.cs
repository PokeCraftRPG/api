using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPokemonRoster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInParty",
                schema: "Pokemon",
                table: "Specimens",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                schema: "Pokemon",
                table: "Specimens",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_WorldId_IsInParty",
                schema: "Pokemon",
                table: "Specimens",
                columns: new[] { "WorldId", "IsInParty" });

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_WorldId_Priority",
                schema: "Pokemon",
                table: "Specimens",
                columns: new[] { "WorldId", "Priority" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Specimens_WorldId_IsInParty",
                schema: "Pokemon",
                table: "Specimens");

            migrationBuilder.DropIndex(
                name: "IX_Specimens_WorldId_Priority",
                schema: "Pokemon",
                table: "Specimens");

            migrationBuilder.DropColumn(
                name: "IsInParty",
                schema: "Pokemon",
                table: "Specimens");

            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "Pokemon",
                table: "Specimens");
        }
    }
}
