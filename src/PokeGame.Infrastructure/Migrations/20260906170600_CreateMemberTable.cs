using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateMemberTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Members",
                schema: "Pokemon",
                columns: table => new
                {
                    WorldId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    GrantedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    GrantedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => new { x.WorldId, x.UserId });
                    table.ForeignKey(
                        name: "FK_Members_Worlds_WorldId",
                        column: x => x.WorldId,
                        principalSchema: "Pokemon",
                        principalTable: "Worlds",
                        principalColumn: "WorldId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Members_GrantedBy",
                schema: "Pokemon",
                table: "Members",
                column: "GrantedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Members_GrantedOn",
                schema: "Pokemon",
                table: "Members",
                column: "GrantedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Members_UserId",
                schema: "Pokemon",
                table: "Members",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Members",
                schema: "Pokemon");
        }
    }
}
