using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PokeGame.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Pokemon");

            migrationBuilder.CreateTable(
                name: "Worlds",
                schema: "Pokemon",
                columns: table => new
                {
                    WorldId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    StreamId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Worlds", x => x.WorldId);
                });

            migrationBuilder.CreateTable(
                name: "Abilities",
                schema: "Pokemon",
                columns: table => new
                {
                    AbilityId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorldId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_Abilities", x => x.AbilityId);
                    table.ForeignKey(
                        name: "FK_Abilities_Worlds_WorldId",
                        column: x => x.WorldId,
                        principalSchema: "Pokemon",
                        principalTable: "Worlds",
                        principalColumn: "WorldId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Moves",
                schema: "Pokemon",
                columns: table => new
                {
                    MoveId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorldId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Category = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Accuracy = table.Column<byte>(type: "smallint", nullable: true),
                    Power = table.Column<byte>(type: "smallint", nullable: true),
                    PowerPoints = table.Column<byte>(type: "smallint", nullable: false),
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
                    table.PrimaryKey("PK_Moves", x => x.MoveId);
                    table.ForeignKey(
                        name: "FK_Moves_Worlds_WorldId",
                        column: x => x.WorldId,
                        principalSchema: "Pokemon",
                        principalTable: "Worlds",
                        principalColumn: "WorldId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Regions",
                schema: "Pokemon",
                columns: table => new
                {
                    RegionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorldId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                name: "IX_Abilities_CreatedBy",
                schema: "Pokemon",
                table: "Abilities",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Abilities_CreatedOn",
                schema: "Pokemon",
                table: "Abilities",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Abilities_StreamId",
                schema: "Pokemon",
                table: "Abilities",
                column: "StreamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Abilities_UpdatedBy",
                schema: "Pokemon",
                table: "Abilities",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Abilities_UpdatedOn",
                schema: "Pokemon",
                table: "Abilities",
                column: "UpdatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Abilities_Version",
                schema: "Pokemon",
                table: "Abilities",
                column: "Version");

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
                name: "IX_Moves_CreatedBy",
                schema: "Pokemon",
                table: "Moves",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Moves_CreatedOn",
                schema: "Pokemon",
                table: "Moves",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Moves_StreamId",
                schema: "Pokemon",
                table: "Moves",
                column: "StreamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Moves_UpdatedBy",
                schema: "Pokemon",
                table: "Moves",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Moves_UpdatedOn",
                schema: "Pokemon",
                table: "Moves",
                column: "UpdatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Moves_Version",
                schema: "Pokemon",
                table: "Moves",
                column: "Version");

            migrationBuilder.CreateIndex(
                name: "IX_Moves_WorldId_Accuracy",
                schema: "Pokemon",
                table: "Moves",
                columns: new[] { "WorldId", "Accuracy" });

            migrationBuilder.CreateIndex(
                name: "IX_Moves_WorldId_Category",
                schema: "Pokemon",
                table: "Moves",
                columns: new[] { "WorldId", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_Moves_WorldId_Id",
                schema: "Pokemon",
                table: "Moves",
                columns: new[] { "WorldId", "Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Moves_WorldId_Key",
                schema: "Pokemon",
                table: "Moves",
                columns: new[] { "WorldId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Moves_WorldId_Name",
                schema: "Pokemon",
                table: "Moves",
                columns: new[] { "WorldId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Moves_WorldId_Power",
                schema: "Pokemon",
                table: "Moves",
                columns: new[] { "WorldId", "Power" });

            migrationBuilder.CreateIndex(
                name: "IX_Moves_WorldId_PowerPoints",
                schema: "Pokemon",
                table: "Moves",
                columns: new[] { "WorldId", "PowerPoints" });

            migrationBuilder.CreateIndex(
                name: "IX_Moves_WorldId_Type",
                schema: "Pokemon",
                table: "Moves",
                columns: new[] { "WorldId", "Type" });

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
                name: "IX_Worlds_CreatedBy",
                schema: "Pokemon",
                table: "Worlds",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Worlds_CreatedOn",
                schema: "Pokemon",
                table: "Worlds",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Worlds_Id",
                schema: "Pokemon",
                table: "Worlds",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Worlds_Key",
                schema: "Pokemon",
                table: "Worlds",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Worlds_Name",
                schema: "Pokemon",
                table: "Worlds",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Worlds_OwnerId",
                schema: "Pokemon",
                table: "Worlds",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Worlds_StreamId",
                schema: "Pokemon",
                table: "Worlds",
                column: "StreamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Worlds_UpdatedBy",
                schema: "Pokemon",
                table: "Worlds",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Worlds_UpdatedOn",
                schema: "Pokemon",
                table: "Worlds",
                column: "UpdatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Worlds_Version",
                schema: "Pokemon",
                table: "Worlds",
                column: "Version");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Abilities",
                schema: "Pokemon");

            migrationBuilder.DropTable(
                name: "Moves",
                schema: "Pokemon");

            migrationBuilder.DropTable(
                name: "Regions",
                schema: "Pokemon");

            migrationBuilder.DropTable(
                name: "Worlds",
                schema: "Pokemon");
        }
    }
}
