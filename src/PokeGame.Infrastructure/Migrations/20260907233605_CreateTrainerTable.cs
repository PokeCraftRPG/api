using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PokeGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateTrainerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Trainers",
                schema: "Pokemon",
                columns: table => new
                {
                    TrainerId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorldId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Summary = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    License = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Gender = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    Money = table.Column<int>(type: "integer", nullable: false),
                    SpriteId = table.Column<int>(type: "integer", nullable: true),
                    MemberId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    StreamId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trainers", x => x.TrainerId);
                    table.ForeignKey(
                        name: "FK_Trainers_Assets_SpriteId",
                        column: x => x.SpriteId,
                        principalSchema: "Pokemon",
                        principalTable: "Assets",
                        principalColumn: "AssetId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trainers_Worlds_WorldId",
                        column: x => x.WorldId,
                        principalSchema: "Pokemon",
                        principalTable: "Worlds",
                        principalColumn: "WorldId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_CreatedBy",
                schema: "Pokemon",
                table: "Trainers",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_CreatedOn",
                schema: "Pokemon",
                table: "Trainers",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_MemberId",
                schema: "Pokemon",
                table: "Trainers",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_SpriteId",
                schema: "Pokemon",
                table: "Trainers",
                column: "SpriteId");

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_StreamId",
                schema: "Pokemon",
                table: "Trainers",
                column: "StreamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_UpdatedBy",
                schema: "Pokemon",
                table: "Trainers",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_UpdatedOn",
                schema: "Pokemon",
                table: "Trainers",
                column: "UpdatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_Version",
                schema: "Pokemon",
                table: "Trainers",
                column: "Version");

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_WorldId_Gender",
                schema: "Pokemon",
                table: "Trainers",
                columns: new[] { "WorldId", "Gender" });

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_WorldId_Id",
                schema: "Pokemon",
                table: "Trainers",
                columns: new[] { "WorldId", "Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_WorldId_Key",
                schema: "Pokemon",
                table: "Trainers",
                columns: new[] { "WorldId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_WorldId_License",
                schema: "Pokemon",
                table: "Trainers",
                columns: new[] { "WorldId", "License" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_WorldId_MemberId",
                schema: "Pokemon",
                table: "Trainers",
                columns: new[] { "WorldId", "MemberId" });

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_WorldId_Money",
                schema: "Pokemon",
                table: "Trainers",
                columns: new[] { "WorldId", "Money" });

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_WorldId_Name",
                schema: "Pokemon",
                table: "Trainers",
                columns: new[] { "WorldId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_WorldId_Summary",
                schema: "Pokemon",
                table: "Trainers",
                columns: new[] { "WorldId", "Summary" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trainers",
                schema: "Pokemon");
        }
    }
}
