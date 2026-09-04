using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PokeGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateAssetTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Assets",
                schema: "Pokemon",
                columns: table => new
                {
                    AssetId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorldId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FileExtension = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    FileMimeType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    StreamId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.AssetId);
                    table.ForeignKey(
                        name: "FK_Assets_Worlds_WorldId",
                        column: x => x.WorldId,
                        principalSchema: "Pokemon",
                        principalTable: "Worlds",
                        principalColumn: "WorldId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_CreatedBy",
                schema: "Pokemon",
                table: "Assets",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_CreatedOn",
                schema: "Pokemon",
                table: "Assets",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_StreamId",
                schema: "Pokemon",
                table: "Assets",
                column: "StreamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_UpdatedBy",
                schema: "Pokemon",
                table: "Assets",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_UpdatedOn",
                schema: "Pokemon",
                table: "Assets",
                column: "UpdatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Version",
                schema: "Pokemon",
                table: "Assets",
                column: "Version");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_WorldId_Duration",
                schema: "Pokemon",
                table: "Assets",
                columns: new[] { "WorldId", "Duration" });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_WorldId_FileExtension",
                schema: "Pokemon",
                table: "Assets",
                columns: new[] { "WorldId", "FileExtension" });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_WorldId_FileMimeType",
                schema: "Pokemon",
                table: "Assets",
                columns: new[] { "WorldId", "FileMimeType" });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_WorldId_FileName",
                schema: "Pokemon",
                table: "Assets",
                columns: new[] { "WorldId", "FileName" });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_WorldId_FileSize",
                schema: "Pokemon",
                table: "Assets",
                columns: new[] { "WorldId", "FileSize" });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_WorldId_Height",
                schema: "Pokemon",
                table: "Assets",
                columns: new[] { "WorldId", "Height" });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_WorldId_Id",
                schema: "Pokemon",
                table: "Assets",
                columns: new[] { "WorldId", "Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_WorldId_Kind",
                schema: "Pokemon",
                table: "Assets",
                columns: new[] { "WorldId", "Kind" });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_WorldId_Width",
                schema: "Pokemon",
                table: "Assets",
                columns: new[] { "WorldId", "Width" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assets",
                schema: "Pokemon");
        }
    }
}
