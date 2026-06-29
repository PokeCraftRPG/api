using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PokeGame.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class CreateRegionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_Worlds_Id",
                schema: "Pokemon",
                table: "Worlds",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Regions",
                schema: "Pokemon",
                columns: table => new
                {
                    RegionId = table.Column<int>(type: "integer", nullable: false)
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
                    table.PrimaryKey("PK_Regions", x => x.RegionId);
                    table.ForeignKey(
                        name: "FK_Regions_Worlds_WorldId",
                        column: x => x.WorldId,
                        principalSchema: "Pokemon",
                        principalTable: "Worlds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Regions_WorldId_CreatedBy",
                schema: "Pokemon",
                table: "Regions",
                columns: new[] { "WorldId", "CreatedBy" });

            migrationBuilder.CreateIndex(
                name: "IX_Regions_WorldId_CreatedOn",
                schema: "Pokemon",
                table: "Regions",
                columns: new[] { "WorldId", "CreatedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_Regions_WorldId_Id",
                schema: "Pokemon",
                table: "Regions",
                columns: new[] { "WorldId", "Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Regions_WorldId_Key",
                schema: "Pokemon",
                table: "Regions",
                columns: new[] { "WorldId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Regions_WorldId_Name",
                schema: "Pokemon",
                table: "Regions",
                columns: new[] { "WorldId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Regions_WorldId_UpdatedBy",
                schema: "Pokemon",
                table: "Regions",
                columns: new[] { "WorldId", "UpdatedBy" });

            migrationBuilder.CreateIndex(
                name: "IX_Regions_WorldId_UpdatedOn",
                schema: "Pokemon",
                table: "Regions",
                columns: new[] { "WorldId", "UpdatedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_Regions_WorldId_Version",
                schema: "Pokemon",
                table: "Regions",
                columns: new[] { "WorldId", "Version" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Regions",
                schema: "Pokemon");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Worlds_Id",
                schema: "Pokemon",
                table: "Worlds");
        }
    }
}
