using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logis.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIgnoredAtUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "IgnoredAtUtc",
                table: "OutBoxMessages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutBoxMessages_IgnoredAtUtc",
                table: "OutBoxMessages",
                column: "IgnoredAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutBoxMessages_IgnoredAtUtc",
                table: "OutBoxMessages");

            migrationBuilder.DropColumn(
                name: "IgnoredAtUtc",
                table: "OutBoxMessages");
        }
    }
}
