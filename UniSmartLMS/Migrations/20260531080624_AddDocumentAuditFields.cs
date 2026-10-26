using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniSmartLMS.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Documents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Documents",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Documents");
        }
    }
}
