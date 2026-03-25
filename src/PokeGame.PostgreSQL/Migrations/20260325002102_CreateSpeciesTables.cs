using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PokeGame.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class CreateSpeciesTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Species",
                schema: "Pokemon",
                columns: table => new
                {
                    SpeciesId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorldId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BaseFriendship = table.Column<byte>(type: "smallint", nullable: false),
                    CatchRate = table.Column<byte>(type: "smallint", nullable: false),
                    GrowthRate = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    EggCycles = table.Column<byte>(type: "smallint", nullable: false),
                    PrimaryEggGroup = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    SecondaryEggGroup = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
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
                    table.PrimaryKey("PK_Species", x => x.SpeciesId);
                    table.ForeignKey(
                        name: "FK_Species_Worlds_WorldId",
                        column: x => x.WorldId,
                        principalSchema: "Pokemon",
                        principalTable: "Worlds",
                        principalColumn: "WorldId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegionalNumbers",
                schema: "Pokemon",
                columns: table => new
                {
                    SpeciesId = table.Column<int>(type: "integer", nullable: false),
                    RegionId = table.Column<int>(type: "integer", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegionalNumbers", x => new { x.SpeciesId, x.RegionId });
                    table.ForeignKey(
                        name: "FK_RegionalNumbers_Regions_RegionId",
                        column: x => x.RegionId,
                        principalSchema: "Pokemon",
                        principalTable: "Regions",
                        principalColumn: "RegionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegionalNumbers_Species_SpeciesId",
                        column: x => x.SpeciesId,
                        principalSchema: "Pokemon",
                        principalTable: "Species",
                        principalColumn: "SpeciesId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegionalNumbers_RegionId_Number",
                schema: "Pokemon",
                table: "RegionalNumbers",
                columns: new[] { "RegionId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Species_CreatedBy",
                schema: "Pokemon",
                table: "Species",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Species_CreatedOn",
                schema: "Pokemon",
                table: "Species",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Species_StreamId",
                schema: "Pokemon",
                table: "Species",
                column: "StreamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Species_UpdatedBy",
                schema: "Pokemon",
                table: "Species",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Species_UpdatedOn",
                schema: "Pokemon",
                table: "Species",
                column: "UpdatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Species_Version",
                schema: "Pokemon",
                table: "Species",
                column: "Version");

            migrationBuilder.CreateIndex(
                name: "IX_Species_WorldId_BaseFriendship",
                schema: "Pokemon",
                table: "Species",
                columns: new[] { "WorldId", "BaseFriendship" });

            migrationBuilder.CreateIndex(
                name: "IX_Species_WorldId_CatchRate",
                schema: "Pokemon",
                table: "Species",
                columns: new[] { "WorldId", "CatchRate" });

            migrationBuilder.CreateIndex(
                name: "IX_Species_WorldId_Category",
                schema: "Pokemon",
                table: "Species",
                columns: new[] { "WorldId", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_Species_WorldId_EggCycles",
                schema: "Pokemon",
                table: "Species",
                columns: new[] { "WorldId", "EggCycles" });

            migrationBuilder.CreateIndex(
                name: "IX_Species_WorldId_GrowthRate",
                schema: "Pokemon",
                table: "Species",
                columns: new[] { "WorldId", "GrowthRate" });

            migrationBuilder.CreateIndex(
                name: "IX_Species_WorldId_Id",
                schema: "Pokemon",
                table: "Species",
                columns: new[] { "WorldId", "Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Species_WorldId_Key",
                schema: "Pokemon",
                table: "Species",
                columns: new[] { "WorldId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Species_WorldId_Name",
                schema: "Pokemon",
                table: "Species",
                columns: new[] { "WorldId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Species_WorldId_Number",
                schema: "Pokemon",
                table: "Species",
                columns: new[] { "WorldId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Species_WorldId_PrimaryEggGroup",
                schema: "Pokemon",
                table: "Species",
                columns: new[] { "WorldId", "PrimaryEggGroup" });

            migrationBuilder.CreateIndex(
                name: "IX_Species_WorldId_SecondaryEggGroup",
                schema: "Pokemon",
                table: "Species",
                columns: new[] { "WorldId", "SecondaryEggGroup" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegionalNumbers",
                schema: "Pokemon");

            migrationBuilder.DropTable(
                name: "Species",
                schema: "Pokemon");
        }
    }
}
