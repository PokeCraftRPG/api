using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PokeGame.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class CreateMoveTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Moves",
                schema: "Pokemon",
                columns: table => new
                {
                    MoveId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorldId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Category = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Accuracy = table.Column<int>(type: "integer", nullable: true),
                    Power = table.Column<int>(type: "integer", nullable: true),
                    PowerPoints = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Moves", x => x.MoveId);
                    table.ForeignKey(
                        name: "FK_Moves_Worlds_WorldId",
                        column: x => x.WorldId,
                        principalSchema: "Pokemon",
                        principalTable: "Worlds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Moves_WorldId_Accuracy",
                schema: "Pokemon",
                table: "Moves",
                columns: new[] { "WorldId", "Accuracy" });

            migrationBuilder.CreateIndex(
                name: "IX_Moves_WorldId_Category",
                schema: "Pokemon",
                table: "Moves",
                columns: new[] { "WorldId", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_Moves_WorldId_CreatedBy",
                schema: "Pokemon",
                table: "Moves",
                columns: new[] { "WorldId", "CreatedBy" });

            migrationBuilder.CreateIndex(
                name: "IX_Moves_WorldId_CreatedOn",
                schema: "Pokemon",
                table: "Moves",
                columns: new[] { "WorldId", "CreatedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_Moves_WorldId_Id",
                schema: "Pokemon",
                table: "Moves",
                columns: new[] { "WorldId", "Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Moves_WorldId_Key",
                schema: "Pokemon",
                table: "Moves",
                columns: new[] { "WorldId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Moves_WorldId_Name",
                schema: "Pokemon",
                table: "Moves",
                columns: new[] { "WorldId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Moves_WorldId_Power",
                schema: "Pokemon",
                table: "Moves",
                columns: new[] { "WorldId", "Power" });

            migrationBuilder.CreateIndex(
                name: "IX_Moves_WorldId_PowerPoints",
                schema: "Pokemon",
                table: "Moves",
                columns: new[] { "WorldId", "PowerPoints" });

            migrationBuilder.CreateIndex(
                name: "IX_Moves_WorldId_Type",
                schema: "Pokemon",
                table: "Moves",
                columns: new[] { "WorldId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Moves_WorldId_UpdatedBy",
                schema: "Pokemon",
                table: "Moves",
                columns: new[] { "WorldId", "UpdatedBy" });

            migrationBuilder.CreateIndex(
                name: "IX_Moves_WorldId_UpdatedOn",
                schema: "Pokemon",
                table: "Moves",
                columns: new[] { "WorldId", "UpdatedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_Moves_WorldId_Version",
                schema: "Pokemon",
                table: "Moves",
                columns: new[] { "WorldId", "Version" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Moves",
                schema: "Pokemon");
        }
    }
}
