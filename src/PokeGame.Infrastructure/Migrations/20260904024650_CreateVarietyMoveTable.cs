using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PokeGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateVarietyMoveTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VarietyMoves",
                schema: "Pokemon",
                columns: table => new
                {
                    VarietyMoveId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VarietyId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MoveId = table.Column<int>(type: "integer", nullable: false),
                    LearningMethod = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VarietyMoves", x => x.VarietyMoveId);
                    table.ForeignKey(
                        name: "FK_VarietyMoves_Moves_MoveId",
                        column: x => x.MoveId,
                        principalSchema: "Pokemon",
                        principalTable: "Moves",
                        principalColumn: "MoveId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VarietyMoves_Varieties_VarietyId",
                        column: x => x.VarietyId,
                        principalSchema: "Pokemon",
                        principalTable: "Varieties",
                        principalColumn: "VarietyId",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_VarietyMoves_MoveId",
                schema: "Pokemon",
                table: "VarietyMoves",
                column: "MoveId");

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
                name: "IX_VarietyMoves_VarietyId_Id",
                schema: "Pokemon",
                table: "VarietyMoves",
                columns: new[] { "VarietyId", "Id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VarietyMoves",
                schema: "Pokemon");
        }
    }
}
