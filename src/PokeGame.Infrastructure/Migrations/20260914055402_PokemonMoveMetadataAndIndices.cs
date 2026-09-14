using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PokemonMoveMetadataAndIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VarietyMoves_CreatedBy",
                schema: "Pokemon",
                table: "VarietyMoves");

            migrationBuilder.DropIndex(
                name: "IX_VarietyMoves_CreatedOn",
                schema: "Pokemon",
                table: "VarietyMoves");

            migrationBuilder.DropIndex(
                name: "IX_VarietyMoves_LearningMethod",
                schema: "Pokemon",
                table: "VarietyMoves");

            migrationBuilder.DropIndex(
                name: "IX_VarietyMoves_Level",
                schema: "Pokemon",
                table: "VarietyMoves");

            migrationBuilder.DropIndex(
                name: "IX_VarietyMoves_UpdatedBy",
                schema: "Pokemon",
                table: "VarietyMoves");

            migrationBuilder.DropIndex(
                name: "IX_VarietyMoves_UpdatedOn",
                schema: "Pokemon",
                table: "VarietyMoves");

            migrationBuilder.DropIndex(
                name: "IX_RegionalNumbers_CreatedBy",
                schema: "Pokemon",
                table: "RegionalNumbers");

            migrationBuilder.DropIndex(
                name: "IX_RegionalNumbers_CreatedOn",
                schema: "Pokemon",
                table: "RegionalNumbers");

            migrationBuilder.DropIndex(
                name: "IX_RegionalNumbers_UpdatedBy",
                schema: "Pokemon",
                table: "RegionalNumbers");

            migrationBuilder.DropIndex(
                name: "IX_RegionalNumbers_UpdatedOn",
                schema: "Pokemon",
                table: "RegionalNumbers");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "Pokemon",
                table: "PokemonMoves",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                schema: "Pokemon",
                table: "PokemonMoves",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "Pokemon",
                table: "PokemonMoves",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                schema: "Pokemon",
                table: "PokemonMoves",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "Pokemon",
                table: "PokemonMoves");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                schema: "Pokemon",
                table: "PokemonMoves");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "Pokemon",
                table: "PokemonMoves");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                schema: "Pokemon",
                table: "PokemonMoves");

            migrationBuilder.CreateIndex(
                name: "IX_VarietyMoves_CreatedBy",
                schema: "Pokemon",
                table: "VarietyMoves",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_VarietyMoves_CreatedOn",
                schema: "Pokemon",
                table: "VarietyMoves",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_VarietyMoves_LearningMethod",
                schema: "Pokemon",
                table: "VarietyMoves",
                column: "LearningMethod");

            migrationBuilder.CreateIndex(
                name: "IX_VarietyMoves_Level",
                schema: "Pokemon",
                table: "VarietyMoves",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_VarietyMoves_UpdatedBy",
                schema: "Pokemon",
                table: "VarietyMoves",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_VarietyMoves_UpdatedOn",
                schema: "Pokemon",
                table: "VarietyMoves",
                column: "UpdatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_RegionalNumbers_CreatedBy",
                schema: "Pokemon",
                table: "RegionalNumbers",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RegionalNumbers_CreatedOn",
                schema: "Pokemon",
                table: "RegionalNumbers",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_RegionalNumbers_UpdatedBy",
                schema: "Pokemon",
                table: "RegionalNumbers",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RegionalNumbers_UpdatedOn",
                schema: "Pokemon",
                table: "RegionalNumbers",
                column: "UpdatedOn");
        }
    }
}
