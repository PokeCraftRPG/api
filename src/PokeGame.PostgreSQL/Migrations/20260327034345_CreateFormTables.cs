using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PokeGame.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class CreateFormTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Forms",
                schema: "Pokemon",
                columns: table => new
                {
                    FormId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorldId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VarietyId = table.Column<int>(type: "integer", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsBattleOnly = table.Column<bool>(type: "boolean", nullable: false),
                    IsMega = table.Column<bool>(type: "boolean", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    Weight = table.Column<int>(type: "integer", nullable: false),
                    PrimaryType = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    SecondaryType = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    BaseHP = table.Column<byte>(type: "smallint", nullable: false),
                    BaseAttack = table.Column<byte>(type: "smallint", nullable: false),
                    BaseDefense = table.Column<byte>(type: "smallint", nullable: false),
                    BaseSpecialAttack = table.Column<byte>(type: "smallint", nullable: false),
                    BaseSpecialDefense = table.Column<byte>(type: "smallint", nullable: false),
                    BaseSpeed = table.Column<byte>(type: "smallint", nullable: false),
                    YieldExperience = table.Column<int>(type: "integer", nullable: false),
                    YieldHP = table.Column<int>(type: "integer", nullable: false),
                    YieldAttack = table.Column<int>(type: "integer", nullable: false),
                    YieldDefense = table.Column<int>(type: "integer", nullable: false),
                    YieldSpecialAttack = table.Column<int>(type: "integer", nullable: false),
                    YieldSpecialDefense = table.Column<int>(type: "integer", nullable: false),
                    YieldSpeed = table.Column<int>(type: "integer", nullable: false),
                    SpriteDefault = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    SpriteShiny = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    SpriteAlternative = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    SpriteAlternativeShiny = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
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
                    table.PrimaryKey("PK_Forms", x => x.FormId);
                    table.ForeignKey(
                        name: "FK_Forms_Varieties_VarietyId",
                        column: x => x.VarietyId,
                        principalSchema: "Pokemon",
                        principalTable: "Varieties",
                        principalColumn: "VarietyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Forms_Worlds_WorldId",
                        column: x => x.WorldId,
                        principalSchema: "Pokemon",
                        principalTable: "Worlds",
                        principalColumn: "WorldId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FormAbilities",
                schema: "Pokemon",
                columns: table => new
                {
                    FormId = table.Column<int>(type: "integer", nullable: false),
                    AbilityId = table.Column<int>(type: "integer", nullable: false),
                    Slot = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormAbilities", x => new { x.FormId, x.AbilityId });
                    table.ForeignKey(
                        name: "FK_FormAbilities_Abilities_AbilityId",
                        column: x => x.AbilityId,
                        principalSchema: "Pokemon",
                        principalTable: "Abilities",
                        principalColumn: "AbilityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormAbilities_Forms_FormId",
                        column: x => x.FormId,
                        principalSchema: "Pokemon",
                        principalTable: "Forms",
                        principalColumn: "FormId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FormAbilities_AbilityId",
                schema: "Pokemon",
                table: "FormAbilities",
                column: "AbilityId");

            migrationBuilder.CreateIndex(
                name: "IX_FormAbilities_FormId_Slot",
                schema: "Pokemon",
                table: "FormAbilities",
                columns: new[] { "FormId", "Slot" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Forms_CreatedBy",
                schema: "Pokemon",
                table: "Forms",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Forms_CreatedOn",
                schema: "Pokemon",
                table: "Forms",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Forms_StreamId",
                schema: "Pokemon",
                table: "Forms",
                column: "StreamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Forms_UpdatedBy",
                schema: "Pokemon",
                table: "Forms",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Forms_UpdatedOn",
                schema: "Pokemon",
                table: "Forms",
                column: "UpdatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Forms_VarietyId",
                schema: "Pokemon",
                table: "Forms",
                column: "VarietyId");

            migrationBuilder.CreateIndex(
                name: "IX_Forms_Version",
                schema: "Pokemon",
                table: "Forms",
                column: "Version");

            migrationBuilder.CreateIndex(
                name: "IX_Forms_WorldId_Height",
                schema: "Pokemon",
                table: "Forms",
                columns: new[] { "WorldId", "Height" });

            migrationBuilder.CreateIndex(
                name: "IX_Forms_WorldId_Id",
                schema: "Pokemon",
                table: "Forms",
                columns: new[] { "WorldId", "Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Forms_WorldId_IsBattleOnly",
                schema: "Pokemon",
                table: "Forms",
                columns: new[] { "WorldId", "IsBattleOnly" });

            migrationBuilder.CreateIndex(
                name: "IX_Forms_WorldId_IsMega",
                schema: "Pokemon",
                table: "Forms",
                columns: new[] { "WorldId", "IsMega" });

            migrationBuilder.CreateIndex(
                name: "IX_Forms_WorldId_Key",
                schema: "Pokemon",
                table: "Forms",
                columns: new[] { "WorldId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Forms_WorldId_Name",
                schema: "Pokemon",
                table: "Forms",
                columns: new[] { "WorldId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Forms_WorldId_PrimaryType",
                schema: "Pokemon",
                table: "Forms",
                columns: new[] { "WorldId", "PrimaryType" });

            migrationBuilder.CreateIndex(
                name: "IX_Forms_WorldId_SecondaryType",
                schema: "Pokemon",
                table: "Forms",
                columns: new[] { "WorldId", "SecondaryType" });

            migrationBuilder.CreateIndex(
                name: "IX_Forms_WorldId_VarietyId_IsDefault",
                schema: "Pokemon",
                table: "Forms",
                columns: new[] { "WorldId", "VarietyId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_Forms_WorldId_Weight",
                schema: "Pokemon",
                table: "Forms",
                columns: new[] { "WorldId", "Weight" });

            migrationBuilder.CreateIndex(
                name: "IX_Forms_WorldId_YieldExperience",
                schema: "Pokemon",
                table: "Forms",
                columns: new[] { "WorldId", "YieldExperience" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FormAbilities",
                schema: "Pokemon");

            migrationBuilder.DropTable(
                name: "Forms",
                schema: "Pokemon");
        }
    }
}
