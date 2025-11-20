using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NorthStarET.NextGen.Lms.Infrastructure.Districts.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialDistricts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "districts");

            migrationBuilder.CreateTable(
                name: "AuditRecords",
                schema: "districts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DistrictId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorRole = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    BeforePayload = table.Column<string>(type: "jsonb", nullable: true),
                    AfterPayload = table.Column<string>(type: "jsonb", nullable: true),
                    TimestampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Districts",
                schema: "districts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Suffix = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DistrictAdmins",
                schema: "districts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DistrictId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    InvitedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VerifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistrictAdmins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DistrictAdmins_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalSchema: "districts",
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditRecords_CorrelationId",
                schema: "districts",
                table: "AuditRecords",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditRecords_DistrictId",
                schema: "districts",
                table: "AuditRecords",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditRecords_DistrictId_ActorId",
                schema: "districts",
                table: "AuditRecords",
                columns: new[] { "DistrictId", "ActorId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditRecords_EntityType_EntityId",
                schema: "districts",
                table: "AuditRecords",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditRecords_TimestampUtc",
                schema: "districts",
                table: "AuditRecords",
                column: "TimestampUtc");

            migrationBuilder.CreateIndex(
                name: "IX_DistrictAdmins_DistrictId",
                schema: "districts",
                table: "DistrictAdmins",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_DistrictAdmins_DistrictId_Email_Unique",
                schema: "districts",
                table: "DistrictAdmins",
                columns: new[] { "DistrictId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DistrictAdmins_InvitedAtUtc",
                schema: "districts",
                table: "DistrictAdmins",
                column: "InvitedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_DistrictAdmins_Status",
                schema: "districts",
                table: "DistrictAdmins",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_DeletedAt",
                schema: "districts",
                table: "Districts",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_Suffix_Unique",
                schema: "districts",
                table: "Districts",
                column: "Suffix",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditRecords",
                schema: "districts");

            migrationBuilder.DropTable(
                name: "DistrictAdmins",
                schema: "districts");

            migrationBuilder.DropTable(
                name: "Districts",
                schema: "districts");
        }
    }
}
