using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KhojMarket.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDealCancellationAndTrust : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Phone",
                table: "Users");

            // These columns may already exist in databases updated before this migration.
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Users', N'BlockReason') IS NULL
                    ALTER TABLE [dbo].[Users] ADD [BlockReason] nvarchar(max) NULL;
                IF COL_LENGTH(N'dbo.Users', N'BlockedAt') IS NULL
                    ALTER TABLE [dbo].[Users] ADD [BlockedAt] datetime2 NULL;
                IF COL_LENGTH(N'dbo.Users', N'IsBlocked') IS NULL
                    ALTER TABLE [dbo].[Users] ADD [IsBlocked] bit NOT NULL CONSTRAINT [DF_Users_IsBlocked] DEFAULT (0);
                """);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletedAt",
                table: "MarketplaceDeals",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            // Some databases already have completion fields from earlier manual updates.
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.MarketplaceDeals', N'BuyerConfirmedAt') IS NULL
                    ALTER TABLE [dbo].[MarketplaceDeals] ADD [BuyerConfirmedAt] datetime2 NULL;
                IF COL_LENGTH(N'dbo.MarketplaceDeals', N'CancellationReason') IS NULL
                    ALTER TABLE [dbo].[MarketplaceDeals] ADD [CancellationReason] nvarchar(max) NULL;
                IF COL_LENGTH(N'dbo.MarketplaceDeals', N'CancelledAt') IS NULL
                    ALTER TABLE [dbo].[MarketplaceDeals] ADD [CancelledAt] datetime2 NULL;
                IF COL_LENGTH(N'dbo.MarketplaceDeals', N'CancelledByUserId') IS NULL
                    ALTER TABLE [dbo].[MarketplaceDeals] ADD [CancelledByUserId] uniqueidentifier NULL;
                IF COL_LENGTH(N'dbo.MarketplaceDeals', N'SellerConfirmedAt') IS NULL
                    ALTER TABLE [dbo].[MarketplaceDeals] ADD [SellerConfirmedAt] datetime2 NULL;
                """);

            migrationBuilder.CreateTable(
                name: "MarketplaceComplaints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReporterUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DealId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AdminNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketplaceComplaints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketplaceComplaints_MarketplaceDeals_DealId",
                        column: x => x.DealId,
                        principalTable: "MarketplaceDeals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MarketplaceComplaints_Users_ReportedUserId",
                        column: x => x.ReportedUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MarketplaceComplaints_Users_ReporterUserId",
                        column: x => x.ReporterUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserWarnings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComplaintId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IssuedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWarnings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserWarnings_MarketplaceComplaints_ComplaintId",
                        column: x => x.ComplaintId,
                        principalTable: "MarketplaceComplaints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserWarnings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Phone",
                table: "Users",
                column: "Phone",
                unique: true,
                filter: "[Phone] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceComplaints_DealId",
                table: "MarketplaceComplaints",
                column: "DealId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceComplaints_ReportedUserId",
                table: "MarketplaceComplaints",
                column: "ReportedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceComplaints_ReporterUserId",
                table: "MarketplaceComplaints",
                column: "ReporterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceComplaints_Status",
                table: "MarketplaceComplaints",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_UserWarnings_ComplaintId",
                table: "UserWarnings",
                column: "ComplaintId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserWarnings_UserId",
                table: "UserWarnings",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserWarnings");

            migrationBuilder.DropTable(
                name: "MarketplaceComplaints");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Phone",
                table: "Users");

            // Preserve User block fields: they may predate this migration.

            // Preserve deal fields because they may have existed before this migration.

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletedAt",
                table: "MarketplaceDeals",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Phone",
                table: "Users",
                column: "Phone");
        }
    }
}
