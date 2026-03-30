using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeGame.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainerOwnershipColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                schema: "Pokemon",
                table: "Trainers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "Pokemon",
                table: "Trainers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_WorldId_UserId",
                schema: "Pokemon",
                table: "Trainers",
                columns: new[] { "WorldId", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Trainers_WorldId_UserId",
                schema: "Pokemon",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                schema: "Pokemon",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "Pokemon",
                table: "Trainers");
        }
    }
}
