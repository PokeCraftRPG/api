using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeGame.Infrastructure.Migrations
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
                table: "Specimens",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetAt",
                schema: "Pokemon",
                table: "Specimens",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MetLevel",
                schema: "Pokemon",
                table: "Specimens",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MetOn",
                schema: "Pokemon",
                table: "Specimens",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OriginalTrainerId",
                schema: "Pokemon",
                table: "Specimens",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnershipEvent",
                schema: "Pokemon",
                table: "Specimens",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PokeBallId",
                schema: "Pokemon",
                table: "Specimens",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_CurrentTrainerId",
                schema: "Pokemon",
                table: "Specimens",
                column: "CurrentTrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_OriginalTrainerId",
                schema: "Pokemon",
                table: "Specimens",
                column: "OriginalTrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_PokeBallId",
                schema: "Pokemon",
                table: "Specimens",
                column: "PokeBallId");

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_WorldId_CurrentTrainerId",
                schema: "Pokemon",
                table: "Specimens",
                columns: new[] { "WorldId", "CurrentTrainerId" });

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_WorldId_OriginalTrainerId",
                schema: "Pokemon",
                table: "Specimens",
                columns: new[] { "WorldId", "OriginalTrainerId" });

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_WorldId_OwnershipEvent",
                schema: "Pokemon",
                table: "Specimens",
                columns: new[] { "WorldId", "OwnershipEvent" });

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_WorldId_PokeBallId",
                schema: "Pokemon",
                table: "Specimens",
                columns: new[] { "WorldId", "PokeBallId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Specimens_Items_PokeBallId",
                schema: "Pokemon",
                table: "Specimens",
                column: "PokeBallId",
                principalSchema: "Pokemon",
                principalTable: "Items",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Specimens_Trainers_CurrentTrainerId",
                schema: "Pokemon",
                table: "Specimens",
                column: "CurrentTrainerId",
                principalSchema: "Pokemon",
                principalTable: "Trainers",
                principalColumn: "TrainerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Specimens_Trainers_OriginalTrainerId",
                schema: "Pokemon",
                table: "Specimens",
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
                name: "FK_Specimens_Items_PokeBallId",
                schema: "Pokemon",
                table: "Specimens");

            migrationBuilder.DropForeignKey(
                name: "FK_Specimens_Trainers_CurrentTrainerId",
                schema: "Pokemon",
                table: "Specimens");

            migrationBuilder.DropForeignKey(
                name: "FK_Specimens_Trainers_OriginalTrainerId",
                schema: "Pokemon",
                table: "Specimens");

            migrationBuilder.DropIndex(
                name: "IX_Specimens_CurrentTrainerId",
                schema: "Pokemon",
                table: "Specimens");

            migrationBuilder.DropIndex(
                name: "IX_Specimens_OriginalTrainerId",
                schema: "Pokemon",
                table: "Specimens");

            migrationBuilder.DropIndex(
                name: "IX_Specimens_PokeBallId",
                schema: "Pokemon",
                table: "Specimens");

            migrationBuilder.DropIndex(
                name: "IX_Specimens_WorldId_CurrentTrainerId",
                schema: "Pokemon",
                table: "Specimens");

            migrationBuilder.DropIndex(
                name: "IX_Specimens_WorldId_OriginalTrainerId",
                schema: "Pokemon",
                table: "Specimens");

            migrationBuilder.DropIndex(
                name: "IX_Specimens_WorldId_OwnershipEvent",
                schema: "Pokemon",
                table: "Specimens");

            migrationBuilder.DropIndex(
                name: "IX_Specimens_WorldId_PokeBallId",
                schema: "Pokemon",
                table: "Specimens");

            migrationBuilder.DropColumn(
                name: "CurrentTrainerId",
                schema: "Pokemon",
                table: "Specimens");

            migrationBuilder.DropColumn(
                name: "MetAt",
                schema: "Pokemon",
                table: "Specimens");

            migrationBuilder.DropColumn(
                name: "MetLevel",
                schema: "Pokemon",
                table: "Specimens");

            migrationBuilder.DropColumn(
                name: "MetOn",
                schema: "Pokemon",
                table: "Specimens");

            migrationBuilder.DropColumn(
                name: "OriginalTrainerId",
                schema: "Pokemon",
                table: "Specimens");

            migrationBuilder.DropColumn(
                name: "OwnershipEvent",
                schema: "Pokemon",
                table: "Specimens");

            migrationBuilder.DropColumn(
                name: "PokeBallId",
                schema: "Pokemon",
                table: "Specimens");
        }
    }
}
