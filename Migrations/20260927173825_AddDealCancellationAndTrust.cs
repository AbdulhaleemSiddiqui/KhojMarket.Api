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

            // Keep existing complaint and warning records in databases with earlier schema changes.
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.MarketplaceComplaints', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[MarketplaceComplaints] (
                        [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_MarketplaceComplaints] PRIMARY KEY,
                        [ReporterUserId] uniqueidentifier NOT NULL,
                        [ReportedUserId] uniqueidentifier NOT NULL,
                        [DealId] uniqueidentifier NULL,
                        [Category] nvarchar(50) NOT NULL,
                        [Details] nvarchar(1500) NOT NULL,
                        [Status] nvarchar(30) NOT NULL,
                        [AdminNote] nvarchar(1000) NULL,
                        [ReviewedByUserId] uniqueidentifier NULL,
                        [ReviewedAt] datetime2 NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        CONSTRAINT [FK_MarketplaceComplaints_MarketplaceDeals_DealId] FOREIGN KEY ([DealId]) REFERENCES [dbo].[MarketplaceDeals] ([Id]),
                        CONSTRAINT [FK_MarketplaceComplaints_Users_ReportedUserId] FOREIGN KEY ([ReportedUserId]) REFERENCES [dbo].[Users] ([Id]),
                        CONSTRAINT [FK_MarketplaceComplaints_Users_ReporterUserId] FOREIGN KEY ([ReporterUserId]) REFERENCES [dbo].[Users] ([Id])
                    );
                END;
                IF OBJECT_ID(N'dbo.UserWarnings', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[UserWarnings] (
                        [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_UserWarnings] PRIMARY KEY,
                        [UserId] uniqueidentifier NOT NULL,
                        [ComplaintId] uniqueidentifier NOT NULL,
                        [Reason] nvarchar(1000) NOT NULL,
                        [IssuedByUserId] uniqueidentifier NOT NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        CONSTRAINT [FK_UserWarnings_MarketplaceComplaints_ComplaintId] FOREIGN KEY ([ComplaintId]) REFERENCES [dbo].[MarketplaceComplaints] ([Id]),
                        CONSTRAINT [FK_UserWarnings_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id])
                    );
                END;
                """);

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

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.MarketplaceComplaints') AND name = N'IX_MarketplaceComplaints_DealId')
                    CREATE INDEX [IX_MarketplaceComplaints_DealId] ON [dbo].[MarketplaceComplaints] ([DealId]);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.MarketplaceComplaints') AND name = N'IX_MarketplaceComplaints_ReportedUserId')
                    CREATE INDEX [IX_MarketplaceComplaints_ReportedUserId] ON [dbo].[MarketplaceComplaints] ([ReportedUserId]);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.MarketplaceComplaints') AND name = N'IX_MarketplaceComplaints_ReporterUserId')
                    CREATE INDEX [IX_MarketplaceComplaints_ReporterUserId] ON [dbo].[MarketplaceComplaints] ([ReporterUserId]);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.MarketplaceComplaints') AND name = N'IX_MarketplaceComplaints_Status')
                    CREATE INDEX [IX_MarketplaceComplaints_Status] ON [dbo].[MarketplaceComplaints] ([Status]);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.UserWarnings') AND name = N'IX_UserWarnings_ComplaintId')
                    CREATE UNIQUE INDEX [IX_UserWarnings_ComplaintId] ON [dbo].[UserWarnings] ([ComplaintId]);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.UserWarnings') AND name = N'IX_UserWarnings_UserId')
                    CREATE INDEX [IX_UserWarnings_UserId] ON [dbo].[UserWarnings] ([UserId]);
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Preserve complaint and warning tables, which may predate this migration.

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
