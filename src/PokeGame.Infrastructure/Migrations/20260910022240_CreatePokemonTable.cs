using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PokeGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreatePokemonTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Specimens",
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
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nickname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Summary = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    Gender = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    IsShiny = table.Column<bool>(type: "boolean", nullable: false),
                    TeraType = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    AbilitySlot = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Size = table.Column<byte>(type: "smallint", nullable: false),
                    Nature = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    EggCycles = table.Column<byte>(type: "smallint", nullable: false),
                    GrowthRate = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Experience = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<byte>(type: "smallint", nullable: false),
                    SkillRanks = table.Column<string>(type: "text", nullable: true),
                    BaseHP = table.Column<byte>(type: "smallint", nullable: false),
                    BaseAttack = table.Column<byte>(type: "smallint", nullable: false),
                    BaseDefense = table.Column<byte>(type: "smallint", nullable: false),
                    BaseSpecialAttack = table.Column<byte>(type: "smallint", nullable: false),
                    BaseSpecialDefense = table.Column<byte>(type: "smallint", nullable: false),
                    BaseSpeed = table.Column<byte>(type: "smallint", nullable: false),
                    IndividualHP = table.Column<byte>(type: "smallint", nullable: false),
                    IndividualAttack = table.Column<byte>(type: "smallint", nullable: false),
                    IndividualDefense = table.Column<byte>(type: "smallint", nullable: false),
                    IndividualSpecialAttack = table.Column<byte>(type: "smallint", nullable: false),
                    IndividualSpecialDefense = table.Column<byte>(type: "smallint", nullable: false),
                    IndividualSpeed = table.Column<byte>(type: "smallint", nullable: false),
                    Vitality = table.Column<int>(type: "integer", nullable: false),
                    Stamina = table.Column<int>(type: "integer", nullable: false),
                    Condition = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Friendship = table.Column<byte>(type: "smallint", nullable: false),
                    Characteristic = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    HeldItemId = table.Column<int>(type: "integer", nullable: true),
                    SpriteId = table.Column<int>(type: "integer", nullable: true),
                    StreamId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specimens", x => x.PokemonId);
                    table.ForeignKey(
                        name: "FK_Specimens_Assets_SpriteId",
                        column: x => x.SpriteId,
                        principalSchema: "Pokemon",
                        principalTable: "Assets",
                        principalColumn: "AssetId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Specimens_Forms_FormId",
                        column: x => x.FormId,
                        principalSchema: "Pokemon",
                        principalTable: "Forms",
                        principalColumn: "FormId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Specimens_Items_HeldItemId",
                        column: x => x.HeldItemId,
                        principalSchema: "Pokemon",
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Specimens_Species_SpeciesId",
                        column: x => x.SpeciesId,
                        principalSchema: "Pokemon",
                        principalTable: "Species",
                        principalColumn: "SpeciesId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Specimens_Varieties_VarietyId",
                        column: x => x.VarietyId,
                        principalSchema: "Pokemon",
                        principalTable: "Varieties",
                        principalColumn: "VarietyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Specimens_Worlds_WorldId",
                        column: x => x.WorldId,
                        principalSchema: "Pokemon",
                        principalTable: "Worlds",
                        principalColumn: "WorldId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_CreatedBy",
                schema: "Pokemon",
                table: "Specimens",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_CreatedOn",
                schema: "Pokemon",
                table: "Specimens",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_FormId",
                schema: "Pokemon",
                table: "Specimens",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_HeldItemId",
                schema: "Pokemon",
                table: "Specimens",
                column: "HeldItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_SpeciesId",
                schema: "Pokemon",
                table: "Specimens",
                column: "SpeciesId");

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_SpriteId",
                schema: "Pokemon",
                table: "Specimens",
                column: "SpriteId");

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_StreamId",
                schema: "Pokemon",
                table: "Specimens",
                column: "StreamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_UpdatedBy",
                schema: "Pokemon",
                table: "Specimens",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_UpdatedOn",
                schema: "Pokemon",
                table: "Specimens",
                column: "UpdatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_VarietyId",
                schema: "Pokemon",
                table: "Specimens",
                column: "VarietyId");

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_Version",
                schema: "Pokemon",
                table: "Specimens",
                column: "Version");

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_WorldId_FormId",
                schema: "Pokemon",
                table: "Specimens",
                columns: new[] { "WorldId", "FormId" });

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_WorldId_Gender",
                schema: "Pokemon",
                table: "Specimens",
                columns: new[] { "WorldId", "Gender" });

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_WorldId_HeldItemId",
                schema: "Pokemon",
                table: "Specimens",
                columns: new[] { "WorldId", "HeldItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_WorldId_Id",
                schema: "Pokemon",
                table: "Specimens",
                columns: new[] { "WorldId", "Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_WorldId_IsShiny",
                schema: "Pokemon",
                table: "Specimens",
                columns: new[] { "WorldId", "IsShiny" });

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_WorldId_Key",
                schema: "Pokemon",
                table: "Specimens",
                columns: new[] { "WorldId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_WorldId_Nickname",
                schema: "Pokemon",
                table: "Specimens",
                columns: new[] { "WorldId", "Nickname" });

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_WorldId_SpeciesId",
                schema: "Pokemon",
                table: "Specimens",
                columns: new[] { "WorldId", "SpeciesId" });

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_WorldId_Summary",
                schema: "Pokemon",
                table: "Specimens",
                columns: new[] { "WorldId", "Summary" });

            migrationBuilder.CreateIndex(
                name: "IX_Specimens_WorldId_VarietyId",
                schema: "Pokemon",
                table: "Specimens",
                columns: new[] { "WorldId", "VarietyId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Specimens",
                schema: "Pokemon");
        }
    }
}
