using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PokeGame.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class CompleteMembership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Members",
                schema: "Pokemon");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "Pokemon",
                table: "Worlds",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Membership",
                schema: "Pokemon",
                columns: table => new
                {
                    WorldId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    GrantedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    GrantedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    RevokedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Membership", x => new { x.WorldId, x.UserId });
                    table.ForeignKey(
                        name: "FK_Membership_Worlds_WorldId",
                        column: x => x.WorldId,
                        principalSchema: "Pokemon",
                        principalTable: "Worlds",
                        principalColumn: "WorldId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Worlds_UserId",
                schema: "Pokemon",
                table: "Worlds",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Membership_MemberId",
                schema: "Pokemon",
                table: "Membership",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Membership_WorldId_GrantedBy",
                schema: "Pokemon",
                table: "Membership",
                columns: new[] { "WorldId", "GrantedBy" });

            migrationBuilder.CreateIndex(
                name: "IX_Membership_WorldId_GrantedOn",
                schema: "Pokemon",
                table: "Membership",
                columns: new[] { "WorldId", "GrantedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_Membership_WorldId_IsActive",
                schema: "Pokemon",
                table: "Membership",
                columns: new[] { "WorldId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Membership_WorldId_RevokedBy",
                schema: "Pokemon",
                table: "Membership",
                columns: new[] { "WorldId", "RevokedBy" });

            migrationBuilder.CreateIndex(
                name: "IX_Membership_WorldId_RevokedOn",
                schema: "Pokemon",
                table: "Membership",
                columns: new[] { "WorldId", "RevokedOn" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Membership",
                schema: "Pokemon");

            migrationBuilder.DropIndex(
                name: "IX_Worlds_UserId",
                schema: "Pokemon",
                table: "Worlds");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "Pokemon",
                table: "Worlds");

            migrationBuilder.CreateTable(
                name: "Members",
                schema: "Pokemon",
                columns: table => new
                {
                    MemberId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorldId = table.Column<int>(type: "integer", nullable: false),
                    GrantedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    GrantedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MemberKey = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    RevokedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    RevokedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.MemberId);
                    table.ForeignKey(
                        name: "FK_Members_Worlds_WorldId",
                        column: x => x.WorldId,
                        principalSchema: "Pokemon",
                        principalTable: "Worlds",
                        principalColumn: "WorldId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Members_MemberKey",
                schema: "Pokemon",
                table: "Members",
                column: "MemberKey");

            migrationBuilder.CreateIndex(
                name: "IX_Members_WorldId_GrantedBy",
                schema: "Pokemon",
                table: "Members",
                columns: new[] { "WorldId", "GrantedBy" });

            migrationBuilder.CreateIndex(
                name: "IX_Members_WorldId_GrantedOn",
                schema: "Pokemon",
                table: "Members",
                columns: new[] { "WorldId", "GrantedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_Members_WorldId_RevokedBy",
                schema: "Pokemon",
                table: "Members",
                columns: new[] { "WorldId", "RevokedBy" });

            migrationBuilder.CreateIndex(
                name: "IX_Members_WorldId_RevokedOn",
                schema: "Pokemon",
                table: "Members",
                columns: new[] { "WorldId", "RevokedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_Members_WorldId_UserId",
                schema: "Pokemon",
                table: "Members",
                columns: new[] { "WorldId", "UserId" },
                unique: true);
        }
    }
}
