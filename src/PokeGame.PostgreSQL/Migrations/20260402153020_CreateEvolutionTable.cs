using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PokeGame.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class CreateEvolutionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Evolutions",
                schema: "Pokemon",
                columns: table => new
                {
                    EvolutionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorldId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceId = table.Column<int>(type: "integer", nullable: false),
                    TargetId = table.Column<int>(type: "integer", nullable: false),
                    Trigger = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: true),
                    Level = table.Column<int>(type: "integer", nullable: true),
                    Friendship = table.Column<bool>(type: "boolean", nullable: false),
                    Gender = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    HeldItemId = table.Column<int>(type: "integer", nullable: true),
                    KnownMoveId = table.Column<int>(type: "integer", nullable: true),
                    Location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TimeOfDay = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    StreamId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evolutions", x => x.EvolutionId);
                    table.ForeignKey(
                        name: "FK_Evolutions_Forms_SourceId",
                        column: x => x.SourceId,
                        principalSchema: "Pokemon",
                        principalTable: "Forms",
                        principalColumn: "FormId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Evolutions_Forms_TargetId",
                        column: x => x.TargetId,
                        principalSchema: "Pokemon",
                        principalTable: "Forms",
                        principalColumn: "FormId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Evolutions_Items_HeldItemId",
                        column: x => x.HeldItemId,
                        principalSchema: "Pokemon",
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Evolutions_Items_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Pokemon",
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Evolutions_Moves_KnownMoveId",
                        column: x => x.KnownMoveId,
                        principalSchema: "Pokemon",
                        principalTable: "Moves",
                        principalColumn: "MoveId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Evolutions_Worlds_WorldId",
                        column: x => x.WorldId,
                        principalSchema: "Pokemon",
                        principalTable: "Worlds",
                        principalColumn: "WorldId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Evolutions_CreatedBy",
                schema: "Pokemon",
                table: "Evolutions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Evolutions_CreatedOn",
                schema: "Pokemon",
                table: "Evolutions",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Evolutions_HeldItemId",
                schema: "Pokemon",
                table: "Evolutions",
                column: "HeldItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Evolutions_ItemId",
                schema: "Pokemon",
                table: "Evolutions",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Evolutions_KnownMoveId",
                schema: "Pokemon",
                table: "Evolutions",
                column: "KnownMoveId");

            migrationBuilder.CreateIndex(
                name: "IX_Evolutions_SourceId",
                schema: "Pokemon",
                table: "Evolutions",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Evolutions_StreamId",
                schema: "Pokemon",
                table: "Evolutions",
                column: "StreamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Evolutions_TargetId",
                schema: "Pokemon",
                table: "Evolutions",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_Evolutions_UpdatedBy",
                schema: "Pokemon",
                table: "Evolutions",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Evolutions_UpdatedOn",
                schema: "Pokemon",
                table: "Evolutions",
                column: "UpdatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Evolutions_Version",
                schema: "Pokemon",
                table: "Evolutions",
                column: "Version");

            migrationBuilder.CreateIndex(
                name: "IX_Evolutions_WorldId_Friendship",
                schema: "Pokemon",
                table: "Evolutions",
                columns: new[] { "WorldId", "Friendship" });

            migrationBuilder.CreateIndex(
                name: "IX_Evolutions_WorldId_Gender",
                schema: "Pokemon",
                table: "Evolutions",
                columns: new[] { "WorldId", "Gender" });

            migrationBuilder.CreateIndex(
                name: "IX_Evolutions_WorldId_HeldItemId",
                schema: "Pokemon",
                table: "Evolutions",
                columns: new[] { "WorldId", "HeldItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_Evolutions_WorldId_Id",
                schema: "Pokemon",
                table: "Evolutions",
                columns: new[] { "WorldId", "Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Evolutions_WorldId_ItemId",
                schema: "Pokemon",
                table: "Evolutions",
                columns: new[] { "WorldId", "ItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_Evolutions_WorldId_KnownMoveId",
                schema: "Pokemon",
                table: "Evolutions",
                columns: new[] { "WorldId", "KnownMoveId" });

            migrationBuilder.CreateIndex(
                name: "IX_Evolutions_WorldId_Level",
                schema: "Pokemon",
                table: "Evolutions",
                columns: new[] { "WorldId", "Level" });

            migrationBuilder.CreateIndex(
                name: "IX_Evolutions_WorldId_Location",
                schema: "Pokemon",
                table: "Evolutions",
                columns: new[] { "WorldId", "Location" });

            migrationBuilder.CreateIndex(
                name: "IX_Evolutions_WorldId_SourceId",
                schema: "Pokemon",
                table: "Evolutions",
                columns: new[] { "WorldId", "SourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_Evolutions_WorldId_TargetId",
                schema: "Pokemon",
                table: "Evolutions",
                columns: new[] { "WorldId", "TargetId" });

            migrationBuilder.CreateIndex(
                name: "IX_Evolutions_WorldId_TimeOfDay",
                schema: "Pokemon",
                table: "Evolutions",
                columns: new[] { "WorldId", "TimeOfDay" });

            migrationBuilder.CreateIndex(
                name: "IX_Evolutions_WorldId_Trigger",
                schema: "Pokemon",
                table: "Evolutions",
                columns: new[] { "WorldId", "Trigger" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Evolutions",
                schema: "Pokemon");
        }
    }
}
