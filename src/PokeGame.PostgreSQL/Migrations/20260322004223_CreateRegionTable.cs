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
            migrationBuilder.CreateTable(
                name: "Regions",
                schema: "Pokemon",
                columns: table => new
                {
                    RegionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorldId = table.Column<int>(type: "integer", nullable: false),
                    WorldUid = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    StreamId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
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
                        principalColumn: "WorldId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Regions_CreatedBy",
                schema: "Pokemon",
                table: "Regions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Regions_CreatedOn",
                schema: "Pokemon",
                table: "Regions",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Regions_Name",
                schema: "Pokemon",
                table: "Regions",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Regions_StreamId",
                schema: "Pokemon",
                table: "Regions",
                column: "StreamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Regions_UpdatedBy",
                schema: "Pokemon",
                table: "Regions",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Regions_UpdatedOn",
                schema: "Pokemon",
                table: "Regions",
                column: "UpdatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Regions_Version",
                schema: "Pokemon",
                table: "Regions",
                column: "Version");

            migrationBuilder.CreateIndex(
                name: "IX_Regions_WorldId_Id",
                schema: "Pokemon",
                table: "Regions",
                columns: new[] { "WorldId", "Id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Regions",
                schema: "Pokemon");
        }
    }
}
