using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PokeGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateMemberInvitationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MemberInvitations",
                schema: "Pokemon",
                columns: table => new
                {
                    MemberInvitationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorldId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmailAddress = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UserId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ExpiresOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StreamId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberInvitations", x => x.MemberInvitationId);
                    table.ForeignKey(
                        name: "FK_MemberInvitations_Worlds_WorldId",
                        column: x => x.WorldId,
                        principalSchema: "Pokemon",
                        principalTable: "Worlds",
                        principalColumn: "WorldId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemberInvitations_CreatedBy",
                schema: "Pokemon",
                table: "MemberInvitations",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MemberInvitations_CreatedOn",
                schema: "Pokemon",
                table: "MemberInvitations",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_MemberInvitations_EmailAddress",
                schema: "Pokemon",
                table: "MemberInvitations",
                column: "EmailAddress");

            migrationBuilder.CreateIndex(
                name: "IX_MemberInvitations_ExpiresOn",
                schema: "Pokemon",
                table: "MemberInvitations",
                column: "ExpiresOn");

            migrationBuilder.CreateIndex(
                name: "IX_MemberInvitations_Status",
                schema: "Pokemon",
                table: "MemberInvitations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MemberInvitations_StreamId",
                schema: "Pokemon",
                table: "MemberInvitations",
                column: "StreamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemberInvitations_UpdatedBy",
                schema: "Pokemon",
                table: "MemberInvitations",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MemberInvitations_UpdatedOn",
                schema: "Pokemon",
                table: "MemberInvitations",
                column: "UpdatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_MemberInvitations_UserId",
                schema: "Pokemon",
                table: "MemberInvitations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberInvitations_Version",
                schema: "Pokemon",
                table: "MemberInvitations",
                column: "Version");

            migrationBuilder.CreateIndex(
                name: "IX_MemberInvitations_WorldId_Id",
                schema: "Pokemon",
                table: "MemberInvitations",
                columns: new[] { "WorldId", "Id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberInvitations",
                schema: "Pokemon");
        }
    }
}
