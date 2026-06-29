using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PokeGame.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class CreateAbilityTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Abilities",
                schema: "Pokemon",
                columns: table => new
                {
                    AbilityId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorldId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Abilities", x => x.AbilityId);
                    table.ForeignKey(
                        name: "FK_Abilities_Worlds_WorldId",
                        column: x => x.WorldId,
                        principalSchema: "Pokemon",
                        principalTable: "Worlds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Abilities_WorldId_CreatedBy",
                schema: "Pokemon",
                table: "Abilities",
                columns: new[] { "WorldId", "CreatedBy" });

            migrationBuilder.CreateIndex(
                name: "IX_Abilities_WorldId_CreatedOn",
                schema: "Pokemon",
                table: "Abilities",
                columns: new[] { "WorldId", "CreatedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_Abilities_WorldId_Id",
                schema: "Pokemon",
                table: "Abilities",
                columns: new[] { "WorldId", "Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Abilities_WorldId_Key",
                schema: "Pokemon",
                table: "Abilities",
                columns: new[] { "WorldId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Abilities_WorldId_Name",
                schema: "Pokemon",
                table: "Abilities",
                columns: new[] { "WorldId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Abilities_WorldId_UpdatedBy",
                schema: "Pokemon",
                table: "Abilities",
                columns: new[] { "WorldId", "UpdatedBy" });

            migrationBuilder.CreateIndex(
                name: "IX_Abilities_WorldId_UpdatedOn",
                schema: "Pokemon",
                table: "Abilities",
                columns: new[] { "WorldId", "UpdatedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_Abilities_WorldId_Version",
                schema: "Pokemon",
                table: "Abilities",
                columns: new[] { "WorldId", "Version" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Abilities",
                schema: "Pokemon");
        }
    }
}
