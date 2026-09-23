using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KhojMarket.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRequirementVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Requirements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationMethod",
                table: "Requirements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAt",
                table: "Requirements",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Requirements");

            migrationBuilder.DropColumn(
                name: "VerificationMethod",
                table: "Requirements");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                table: "Requirements");
        }
    }
}
