using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommunityCar.Infrastructure.Data.Migrations
{
    /// <summary>
    /// Migration to add LocalizationResources table for managing translations
    /// </summary>
    public partial class AddLocalizationResourcesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocalizationResources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CultureCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalizationResources", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationResources_Key_CultureCode",
                table: "LocalizationResources",
                columns: new[] { "Key", "CultureCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationResources_Category",
                table: "LocalizationResources",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationResources_CultureCode",
                table: "LocalizationResources",
                column: "CultureCode");

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationResources_IsActive",
                table: "LocalizationResources",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocalizationResources");
        }
    }
}
