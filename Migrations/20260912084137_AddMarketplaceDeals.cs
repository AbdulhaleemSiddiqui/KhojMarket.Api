
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KhojMarket.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketplaceDeals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MarketplaceDeals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequirementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuyerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SellerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InquiryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketplaceDeals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketplaceDeals_Requirements_RequirementId",
                        column: x => x.RequirementId,
                        principalTable: "Requirements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MarketplaceDeals_SellerInquiries_InquiryId",
                        column: x => x.InquiryId,
                        principalTable: "SellerInquiries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MarketplaceDeals_Users_BuyerUserId",
                        column: x => x.BuyerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MarketplaceDeals_Users_SellerUserId",
                        column: x => x.SellerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceDeals_BuyerUserId",
                table: "MarketplaceDeals",
                column: "BuyerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceDeals_InquiryId",
                table: "MarketplaceDeals",
                column: "InquiryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceDeals_RequirementId",
                table: "MarketplaceDeals",
                column: "RequirementId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceDeals_SellerUserId",
                table: "MarketplaceDeals",
                column: "SellerUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketplaceDeals");
        }
    }
}
