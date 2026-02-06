using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommunityCar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityAlertEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SecurityAlerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    AlertType = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AffectedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AffectedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false),
                    ResolvedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResolvedByName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DetectedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityAlerts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAlerts_AffectedUserId",
                table: "SecurityAlerts",
                column: "AffectedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAlerts_AlertType",
                table: "SecurityAlerts",
                column: "AlertType");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAlerts_DetectedAt",
                table: "SecurityAlerts",
                column: "DetectedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAlerts_DetectedAt_IsResolved",
                table: "SecurityAlerts",
                columns: new[] { "DetectedAt", "IsResolved" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAlerts_IsResolved",
                table: "SecurityAlerts",
                column: "IsResolved");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAlerts_Severity",
                table: "SecurityAlerts",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAlerts_Severity_IsResolved",
                table: "SecurityAlerts",
                columns: new[] { "Severity", "IsResolved" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SecurityAlerts");
        }
    }
}
