using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeGame.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddPokemonOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentTrainerId",
                schema: "Pokemon",
                table: "Pokemon",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MetAtLevel",
                schema: "Pokemon",
                table: "Pokemon",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetAtLocation",
                schema: "Pokemon",
                table: "Pokemon",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MetOn",
                schema: "Pokemon",
                table: "Pokemon",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OriginalTrainerId",
                schema: "Pokemon",
                table: "Pokemon",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnershipKind",
                schema: "Pokemon",
                table: "Pokemon",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PokeBallId",
                schema: "Pokemon",
                table: "Pokemon",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_CurrentTrainerId",
                schema: "Pokemon",
                table: "Pokemon",
                column: "CurrentTrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_OriginalTrainerId",
                schema: "Pokemon",
                table: "Pokemon",
                column: "OriginalTrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_PokeBallId",
                schema: "Pokemon",
                table: "Pokemon",
                column: "PokeBallId");

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_WorldId_CurrentTrainerId",
                schema: "Pokemon",
                table: "Pokemon",
                columns: new[] { "WorldId", "CurrentTrainerId" });

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_WorldId_OriginalTrainerId",
                schema: "Pokemon",
                table: "Pokemon",
                columns: new[] { "WorldId", "OriginalTrainerId" });

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_WorldId_OwnershipKind",
                schema: "Pokemon",
                table: "Pokemon",
                columns: new[] { "WorldId", "OwnershipKind" });

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_WorldId_PokeBallId",
                schema: "Pokemon",
                table: "Pokemon",
                columns: new[] { "WorldId", "PokeBallId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Pokemon_Items_PokeBallId",
                schema: "Pokemon",
                table: "Pokemon",
                column: "PokeBallId",
                principalSchema: "Pokemon",
                principalTable: "Items",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pokemon_Trainers_CurrentTrainerId",
                schema: "Pokemon",
                table: "Pokemon",
                column: "CurrentTrainerId",
                principalSchema: "Pokemon",
                principalTable: "Trainers",
                principalColumn: "TrainerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pokemon_Trainers_OriginalTrainerId",
                schema: "Pokemon",
                table: "Pokemon",
                column: "OriginalTrainerId",
                principalSchema: "Pokemon",
                principalTable: "Trainers",
                principalColumn: "TrainerId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pokemon_Items_PokeBallId",
                schema: "Pokemon",
                table: "Pokemon");

            migrationBuilder.DropForeignKey(
                name: "FK_Pokemon_Trainers_CurrentTrainerId",
                schema: "Pokemon",
                table: "Pokemon");

            migrationBuilder.DropForeignKey(
                name: "FK_Pokemon_Trainers_OriginalTrainerId",
                schema: "Pokemon",
                table: "Pokemon");

            migrationBuilder.DropIndex(
                name: "IX_Pokemon_CurrentTrainerId",
                schema: "Pokemon",
                table: "Pokemon");

            migrationBuilder.DropIndex(
                name: "IX_Pokemon_OriginalTrainerId",
                schema: "Pokemon",
                table: "Pokemon");

            migrationBuilder.DropIndex(
                name: "IX_Pokemon_PokeBallId",
                schema: "Pokemon",
                table: "Pokemon");

            migrationBuilder.DropIndex(
                name: "IX_Pokemon_WorldId_CurrentTrainerId",
                schema: "Pokemon",
                table: "Pokemon");

            migrationBuilder.DropIndex(
                name: "IX_Pokemon_WorldId_OriginalTrainerId",
                schema: "Pokemon",
                table: "Pokemon");

            migrationBuilder.DropIndex(
                name: "IX_Pokemon_WorldId_OwnershipKind",
                schema: "Pokemon",
                table: "Pokemon");

            migrationBuilder.DropIndex(
                name: "IX_Pokemon_WorldId_PokeBallId",
                schema: "Pokemon",
                table: "Pokemon");

            migrationBuilder.DropColumn(
                name: "CurrentTrainerId",
                schema: "Pokemon",
                table: "Pokemon");

            migrationBuilder.DropColumn(
                name: "MetAtLevel",
                schema: "Pokemon",
                table: "Pokemon");

            migrationBuilder.DropColumn(
                name: "MetAtLocation",
                schema: "Pokemon",
                table: "Pokemon");

            migrationBuilder.DropColumn(
                name: "MetOn",
                schema: "Pokemon",
                table: "Pokemon");

            migrationBuilder.DropColumn(
                name: "OriginalTrainerId",
                schema: "Pokemon",
                table: "Pokemon");

            migrationBuilder.DropColumn(
                name: "OwnershipKind",
                schema: "Pokemon",
                table: "Pokemon");

            migrationBuilder.DropColumn(
                name: "PokeBallId",
                schema: "Pokemon",
                table: "Pokemon");
        }
    }
}
