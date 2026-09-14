using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPokemonTier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Tier",
                schema: "Pokemon",
                table: "Specimens",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_WorldId_Level",
                schema: "Pokemon",
                table: "Specimens",
                columns: new[] { "WorldId", "Level" });

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_WorldId_MetOn",
                schema: "Pokemon",
                table: "Specimens",
                columns: new[] { "WorldId", "MetOn" });

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_WorldId_Tier",
                schema: "Pokemon",
                table: "Specimens",
                columns: new[] { "WorldId", "Tier" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Specimens_WorldId_Level",
                schema: "Pokemon",
                table: "Specimens");

            migrationBuilder.DropIndex(
                name: "IX_Specimens_WorldId_MetOn",
                schema: "Pokemon",
                table: "Specimens");

            migrationBuilder.DropIndex(
                name: "IX_Specimens_WorldId_Tier",
                schema: "Pokemon",
                table: "Specimens");

            migrationBuilder.DropColumn(
                name: "Tier",
                schema: "Pokemon",
                table: "Specimens");
        }
    }
}
