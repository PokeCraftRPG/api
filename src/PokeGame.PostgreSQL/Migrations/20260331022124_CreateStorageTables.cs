using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PokeGame.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class CreateStorageTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StorageSummary",
                schema: "Pokemon",
                columns: table => new
                {
                    WorldId = table.Column<int>(type: "integer", nullable: false),
                    AllocatedBytes = table.Column<long>(type: "bigint", nullable: false),
                    UsedBytes = table.Column<long>(type: "bigint", nullable: false),
                    RemainingBytes = table.Column<long>(type: "bigint", nullable: false),
                    StreamId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageSummary", x => x.WorldId);
                    table.ForeignKey(
                        name: "FK_StorageSummary_Worlds_WorldId",
                        column: x => x.WorldId,
                        principalSchema: "Pokemon",
                        principalTable: "Worlds",
                        principalColumn: "WorldId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StorageDetail",
                schema: "Pokemon",
                columns: table => new
                {
                    StorageDetailId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SummaryWorldId = table.Column<int>(type: "integer", nullable: true),
                    WorldId = table.Column<int>(type: "integer", nullable: false),
                    EntityKind = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageDetail", x => x.StorageDetailId);
                    table.ForeignKey(
                        name: "FK_StorageDetail_StorageSummary_SummaryWorldId",
                        column: x => x.SummaryWorldId,
                        principalSchema: "Pokemon",
                        principalTable: "StorageSummary",
                        principalColumn: "WorldId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StorageDetail_Key",
                schema: "Pokemon",
                table: "StorageDetail",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StorageDetail_Size",
                schema: "Pokemon",
                table: "StorageDetail",
                column: "Size");

            migrationBuilder.CreateIndex(
                name: "IX_StorageDetail_SummaryWorldId",
                schema: "Pokemon",
                table: "StorageDetail",
                column: "SummaryWorldId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageDetail_WorldId_EntityKind_EntityId",
                schema: "Pokemon",
                table: "StorageDetail",
                columns: new[] { "WorldId", "EntityKind", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_StorageSummary_AllocatedBytes",
                schema: "Pokemon",
                table: "StorageSummary",
                column: "AllocatedBytes");

            migrationBuilder.CreateIndex(
                name: "IX_StorageSummary_CreatedBy",
                schema: "Pokemon",
                table: "StorageSummary",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_StorageSummary_CreatedOn",
                schema: "Pokemon",
                table: "StorageSummary",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_StorageSummary_RemainingBytes",
                schema: "Pokemon",
                table: "StorageSummary",
                column: "RemainingBytes");

            migrationBuilder.CreateIndex(
                name: "IX_StorageSummary_StreamId",
                schema: "Pokemon",
                table: "StorageSummary",
                column: "StreamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StorageSummary_UpdatedBy",
                schema: "Pokemon",
                table: "StorageSummary",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_StorageSummary_UpdatedOn",
                schema: "Pokemon",
                table: "StorageSummary",
                column: "UpdatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_StorageSummary_UsedBytes",
                schema: "Pokemon",
                table: "StorageSummary",
                column: "UsedBytes");

            migrationBuilder.CreateIndex(
                name: "IX_StorageSummary_Version",
                schema: "Pokemon",
                table: "StorageSummary",
                column: "Version");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StorageDetail",
                schema: "Pokemon");

            migrationBuilder.DropTable(
                name: "StorageSummary",
                schema: "Pokemon");
        }
    }
}
