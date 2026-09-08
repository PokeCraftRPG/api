using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainerMemberForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_Trainers_Members_WorldId_MemberId",
                schema: "Pokemon",
                table: "Trainers",
                columns: new[] { "WorldId", "MemberId" },
                principalSchema: "Pokemon",
                principalTable: "Members",
                principalColumns: new[] { "WorldId", "UserId" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trainers_Members_WorldId_MemberId",
                schema: "Pokemon",
                table: "Trainers");
        }
    }
}
