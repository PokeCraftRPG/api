using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GlobalMemberInvitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MemberInvitations_WorldId_Id",
                schema: "Pokemon",
                table: "MemberInvitations");

            migrationBuilder.CreateIndex(
                name: "IX_MemberInvitations_Id",
                schema: "Pokemon",
                table: "MemberInvitations",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemberInvitations_WorldId",
                schema: "Pokemon",
                table: "MemberInvitations",
                column: "WorldId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MemberInvitations_Id",
                schema: "Pokemon",
                table: "MemberInvitations");

            migrationBuilder.DropIndex(
                name: "IX_MemberInvitations_WorldId",
                schema: "Pokemon",
                table: "MemberInvitations");

            migrationBuilder.CreateIndex(
                name: "IX_MemberInvitations_WorldId_Id",
                schema: "Pokemon",
                table: "MemberInvitations",
                columns: new[] { "WorldId", "Id" },
                unique: true);
        }
    }
}
