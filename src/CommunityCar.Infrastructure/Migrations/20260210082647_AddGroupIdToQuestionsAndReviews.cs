using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommunityCar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupIdToQuestionsAndReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GroupId",
                table: "Reviews",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GroupId",
                table: "Questions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_GroupId",
                table: "Reviews",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_GroupId",
                table: "Questions",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_CommunityGroups_GroupId",
                table: "Questions",
                column: "GroupId",
                principalTable: "CommunityGroups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_CommunityGroups_GroupId",
                table: "Reviews",
                column: "GroupId",
                principalTable: "CommunityGroups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_CommunityGroups_GroupId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_CommunityGroups_GroupId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_GroupId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Questions_GroupId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Questions");
        }
    }
}
