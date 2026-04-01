using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PokeGame.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class CreateItemAndMembershipTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Items",
                schema: "Pokemon",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorldId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<int>(type: "integer", nullable: true),
                    Sprite = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    MoveId = table.Column<int>(type: "integer", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    StreamId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_Items_Moves_MoveId",
                        column: x => x.MoveId,
                        principalSchema: "Pokemon",
                        principalTable: "Moves",
                        principalColumn: "MoveId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Items_Worlds_WorldId",
                        column: x => x.WorldId,
                        principalSchema: "Pokemon",
                        principalTable: "Worlds",
                        principalColumn: "WorldId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                schema: "Pokemon",
                columns: table => new
                {
                    MemberId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorldId = table.Column<int>(type: "integer", nullable: false),
                    MemberKey = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    GrantedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    GrantedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    RevokedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "MembershipInvitations",
                schema: "Pokemon",
                columns: table => new
                {
                    MembershipInvitationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorldId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmailAddress = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EmailAddressNormalized = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    InviteeId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
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
                    table.PrimaryKey("PK_MembershipInvitations", x => x.MembershipInvitationId);
                    table.ForeignKey(
                        name: "FK_MembershipInvitations_Worlds_WorldId",
                        column: x => x.WorldId,
                        principalSchema: "Pokemon",
                        principalTable: "Worlds",
                        principalColumn: "WorldId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Items_CreatedBy",
                schema: "Pokemon",
                table: "Items",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Items_CreatedOn",
                schema: "Pokemon",
                table: "Items",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Items_MoveId",
                schema: "Pokemon",
                table: "Items",
                column: "MoveId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_StreamId",
                schema: "Pokemon",
                table: "Items",
                column: "StreamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_UpdatedBy",
                schema: "Pokemon",
                table: "Items",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Items_UpdatedOn",
                schema: "Pokemon",
                table: "Items",
                column: "UpdatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Version",
                schema: "Pokemon",
                table: "Items",
                column: "Version");

            migrationBuilder.CreateIndex(
                name: "IX_Items_WorldId_Category",
                schema: "Pokemon",
                table: "Items",
                columns: new[] { "WorldId", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_Items_WorldId_Id",
                schema: "Pokemon",
                table: "Items",
                columns: new[] { "WorldId", "Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_WorldId_Key",
                schema: "Pokemon",
                table: "Items",
                columns: new[] { "WorldId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_WorldId_MoveId",
                schema: "Pokemon",
                table: "Items",
                columns: new[] { "WorldId", "MoveId" });

            migrationBuilder.CreateIndex(
                name: "IX_Items_WorldId_Name",
                schema: "Pokemon",
                table: "Items",
                columns: new[] { "WorldId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Items_WorldId_Price",
                schema: "Pokemon",
                table: "Items",
                columns: new[] { "WorldId", "Price" });

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

            migrationBuilder.CreateIndex(
                name: "IX_MembershipInvitations_CreatedBy",
                schema: "Pokemon",
                table: "MembershipInvitations",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipInvitations_CreatedOn",
                schema: "Pokemon",
                table: "MembershipInvitations",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipInvitations_InviteeId",
                schema: "Pokemon",
                table: "MembershipInvitations",
                column: "InviteeId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipInvitations_StreamId",
                schema: "Pokemon",
                table: "MembershipInvitations",
                column: "StreamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipInvitations_UpdatedBy",
                schema: "Pokemon",
                table: "MembershipInvitations",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipInvitations_UpdatedOn",
                schema: "Pokemon",
                table: "MembershipInvitations",
                column: "UpdatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipInvitations_Version",
                schema: "Pokemon",
                table: "MembershipInvitations",
                column: "Version");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipInvitations_WorldId_EmailAddressNormalized",
                schema: "Pokemon",
                table: "MembershipInvitations",
                columns: new[] { "WorldId", "EmailAddressNormalized" });

            migrationBuilder.CreateIndex(
                name: "IX_MembershipInvitations_WorldId_ExpiresOn",
                schema: "Pokemon",
                table: "MembershipInvitations",
                columns: new[] { "WorldId", "ExpiresOn" });

            migrationBuilder.CreateIndex(
                name: "IX_MembershipInvitations_WorldId_Id",
                schema: "Pokemon",
                table: "MembershipInvitations",
                columns: new[] { "WorldId", "Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipInvitations_WorldId_Status",
                schema: "Pokemon",
                table: "MembershipInvitations",
                columns: new[] { "WorldId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MembershipInvitations_WorldId_UserId",
                schema: "Pokemon",
                table: "MembershipInvitations",
                columns: new[] { "WorldId", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Items",
                schema: "Pokemon");

            migrationBuilder.DropTable(
                name: "Members",
                schema: "Pokemon");

            migrationBuilder.DropTable(
                name: "MembershipInvitations",
                schema: "Pokemon");
        }
    }
}
