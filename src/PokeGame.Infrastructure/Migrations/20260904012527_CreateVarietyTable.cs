using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PokeGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateVarietyTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Varieties",
                schema: "Pokemon",
                columns: table => new
                {
                    VarietyId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorldId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SpeciesId = table.Column<int>(type: "integer", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Summary = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    CanChangeForm = table.Column<bool>(type: "boolean", nullable: false),
                    GenderRatio = table.Column<int>(type: "integer", nullable: true),
                    Genus = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    StreamId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Varieties", x => x.VarietyId);
                    table.ForeignKey(
                        name: "FK_Varieties_Species_SpeciesId",
                        column: x => x.SpeciesId,
                        principalSchema: "Pokemon",
                        principalTable: "Species",
                        principalColumn: "SpeciesId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Varieties_Worlds_WorldId",
                        column: x => x.WorldId,
                        principalSchema: "Pokemon",
                        principalTable: "Worlds",
                        principalColumn: "WorldId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Varieties_CreatedBy",
                schema: "Pokemon",
                table: "Varieties",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Varieties_CreatedOn",
                schema: "Pokemon",
                table: "Varieties",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Varieties_SpeciesId",
                schema: "Pokemon",
                table: "Varieties",
                column: "SpeciesId");

            migrationBuilder.CreateIndex(
                name: "IX_Varieties_StreamId",
                schema: "Pokemon",
                table: "Varieties",
                column: "StreamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Varieties_UpdatedBy",
                schema: "Pokemon",
                table: "Varieties",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Varieties_UpdatedOn",
                schema: "Pokemon",
                table: "Varieties",
                column: "UpdatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Varieties_Version",
                schema: "Pokemon",
                table: "Varieties",
                column: "Version");

            migrationBuilder.CreateIndex(
                name: "IX_Varieties_WorldId_CanChangeForm",
                schema: "Pokemon",
                table: "Varieties",
                columns: new[] { "WorldId", "CanChangeForm" });

            migrationBuilder.CreateIndex(
                name: "IX_Varieties_WorldId_GenderRatio",
                schema: "Pokemon",
                table: "Varieties",
                columns: new[] { "WorldId", "GenderRatio" });

            migrationBuilder.CreateIndex(
                name: "IX_Varieties_WorldId_Genus",
                schema: "Pokemon",
                table: "Varieties",
                columns: new[] { "WorldId", "Genus" });

            migrationBuilder.CreateIndex(
                name: "IX_Varieties_WorldId_Id",
                schema: "Pokemon",
                table: "Varieties",
                columns: new[] { "WorldId", "Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Varieties_WorldId_IsDefault",
                schema: "Pokemon",
                table: "Varieties",
                columns: new[] { "WorldId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_Varieties_WorldId_Key",
                schema: "Pokemon",
                table: "Varieties",
                columns: new[] { "WorldId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Varieties_WorldId_Name",
                schema: "Pokemon",
                table: "Varieties",
                columns: new[] { "WorldId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Varieties_WorldId_SpeciesId",
                schema: "Pokemon",
                table: "Varieties",
                columns: new[] { "WorldId", "SpeciesId" });

            migrationBuilder.CreateIndex(
                name: "IX_Varieties_WorldId_Summary",
                schema: "Pokemon",
                table: "Varieties",
                columns: new[] { "WorldId", "Summary" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Varieties",
                schema: "Pokemon");
        }
    }
}
