using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KhojMarket.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRequirementFavorites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActivityReason",
                table: "Requirements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "Requirements",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompletedSellerUserId",
                table: "Requirements",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompletionReason",
                table: "Requirements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Requirements",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletionReason",
                table: "Requirements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Requirements",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "RequirementFavorites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequirementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequirementFavorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequirementFavorites_Requirements_RequirementId",
                        column: x => x.RequirementId,
                        principalTable: "Requirements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequirementFavorites_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequirementFavorites_RequirementId",
                table: "RequirementFavorites",
                column: "RequirementId");

            migrationBuilder.CreateIndex(
                name: "IX_RequirementFavorites_UserId_RequirementId",
                table: "RequirementFavorites",
                columns: new[] { "UserId", "RequirementId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequirementFavorites");

            migrationBuilder.DropColumn(
                name: "ActivityReason",
                table: "Requirements");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Requirements");

            migrationBuilder.DropColumn(
                name: "CompletedSellerUserId",
                table: "Requirements");

            migrationBuilder.DropColumn(
                name: "CompletionReason",
                table: "Requirements");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Requirements");

            migrationBuilder.DropColumn(
                name: "DeletionReason",
                table: "Requirements");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Requirements");
        }
    }
}
