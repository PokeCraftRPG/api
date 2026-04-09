using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PokeGame.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class CreatePokemonTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pokemon",
                schema: "Pokemon",
                columns: table => new
                {
                    PokemonId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorldId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SpeciesId = table.Column<int>(type: "integer", nullable: false),
                    VarietyId = table.Column<int>(type: "integer", nullable: false),
                    FormId = table.Column<int>(type: "integer", nullable: false),
                    Key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Gender = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    IsShiny = table.Column<bool>(type: "boolean", nullable: false),
                    TeraType = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Height = table.Column<byte>(type: "smallint", nullable: false),
                    Weight = table.Column<byte>(type: "smallint", nullable: false),
                    Size = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    AbilitySlot = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Nature = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    EggCycles = table.Column<byte>(type: "smallint", nullable: true),
                    IsEgg = table.Column<bool>(type: "boolean", nullable: false),
                    GrowthRate = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Experience = table.Column<int>(type: "integer", nullable: false),
                    MaximumExperience = table.Column<int>(type: "integer", nullable: false),
                    ToNextLevel = table.Column<int>(type: "integer", nullable: false),
                    Statistics = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Vitality = table.Column<int>(type: "integer", nullable: false),
                    Stamina = table.Column<int>(type: "integer", nullable: false),
                    StatusCondition = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Friendship = table.Column<byte>(type: "smallint", nullable: false),
                    Characteristic = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    HeldItemId = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_Pokemon", x => x.PokemonId);
                    table.ForeignKey(
                        name: "FK_Pokemon_Forms_FormId",
                        column: x => x.FormId,
                        principalSchema: "Pokemon",
                        principalTable: "Forms",
                        principalColumn: "FormId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pokemon_Items_HeldItemId",
                        column: x => x.HeldItemId,
                        principalSchema: "Pokemon",
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pokemon_Species_SpeciesId",
                        column: x => x.SpeciesId,
                        principalSchema: "Pokemon",
                        principalTable: "Species",
                        principalColumn: "SpeciesId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pokemon_Varieties_VarietyId",
                        column: x => x.VarietyId,
                        principalSchema: "Pokemon",
                        principalTable: "Varieties",
                        principalColumn: "VarietyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pokemon_Worlds_WorldId",
                        column: x => x.WorldId,
                        principalSchema: "Pokemon",
                        principalTable: "Worlds",
                        principalColumn: "WorldId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_CreatedBy",
                schema: "Pokemon",
                table: "Pokemon",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_CreatedOn",
                schema: "Pokemon",
                table: "Pokemon",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_FormId",
                schema: "Pokemon",
                table: "Pokemon",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_HeldItemId",
                schema: "Pokemon",
                table: "Pokemon",
                column: "HeldItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_SpeciesId",
                schema: "Pokemon",
                table: "Pokemon",
                column: "SpeciesId");

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_StreamId",
                schema: "Pokemon",
                table: "Pokemon",
                column: "StreamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_UpdatedBy",
                schema: "Pokemon",
                table: "Pokemon",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_UpdatedOn",
                schema: "Pokemon",
                table: "Pokemon",
                column: "UpdatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_VarietyId",
                schema: "Pokemon",
                table: "Pokemon",
                column: "VarietyId");

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_Version",
                schema: "Pokemon",
                table: "Pokemon",
                column: "Version");

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_WorldId_FormId",
                schema: "Pokemon",
                table: "Pokemon",
                columns: new[] { "WorldId", "FormId" });

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_WorldId_Gender",
                schema: "Pokemon",
                table: "Pokemon",
                columns: new[] { "WorldId", "Gender" });

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_WorldId_HeldItemId",
                schema: "Pokemon",
                table: "Pokemon",
                columns: new[] { "WorldId", "HeldItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_WorldId_Id",
                schema: "Pokemon",
                table: "Pokemon",
                columns: new[] { "WorldId", "Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_WorldId_IsEgg",
                schema: "Pokemon",
                table: "Pokemon",
                columns: new[] { "WorldId", "IsEgg" });

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_WorldId_IsShiny",
                schema: "Pokemon",
                table: "Pokemon",
                columns: new[] { "WorldId", "IsShiny" });

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_WorldId_Key",
                schema: "Pokemon",
                table: "Pokemon",
                columns: new[] { "WorldId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_WorldId_Level",
                schema: "Pokemon",
                table: "Pokemon",
                columns: new[] { "WorldId", "Level" });

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_WorldId_Name",
                schema: "Pokemon",
                table: "Pokemon",
                columns: new[] { "WorldId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_WorldId_Size",
                schema: "Pokemon",
                table: "Pokemon",
                columns: new[] { "WorldId", "Size" });

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_WorldId_SpeciesId",
                schema: "Pokemon",
                table: "Pokemon",
                columns: new[] { "WorldId", "SpeciesId" });

            migrationBuilder.CreateIndex(
                name: "IX_Pokemon_WorldId_VarietyId",
                schema: "Pokemon",
                table: "Pokemon",
                columns: new[] { "WorldId", "VarietyId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pokemon",
                schema: "Pokemon");
        }
    }
}
