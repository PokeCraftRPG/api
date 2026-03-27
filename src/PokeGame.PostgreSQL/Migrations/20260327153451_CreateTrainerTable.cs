using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PokeGame.PostgreSQL.Migrations
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
                    License = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Gender = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    Money = table.Column<int>(type: "integer", nullable: false),
                    PartySize = table.Column<int>(type: "integer", nullable: false),
                    Sprite = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
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
                    table.PrimaryKey("PK_Trainers", x => x.TrainerId);
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
                name: "IX_Trainers_WorldId_PartySize",
                schema: "Pokemon",
                table: "Trainers",
                columns: new[] { "WorldId", "PartySize" });
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
