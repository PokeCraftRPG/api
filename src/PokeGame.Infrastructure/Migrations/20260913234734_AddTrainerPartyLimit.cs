using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainerPartyLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PartyCount",
                schema: "Pokemon",
                table: "Trainers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PartyLimit",
                schema: "Pokemon",
                table: "Trainers",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PartyCount",
                schema: "Pokemon",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "PartyLimit",
                schema: "Pokemon",
                table: "Trainers");
        }
    }
}
