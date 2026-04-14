using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logis.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeadAtUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeadAtUtc",
                table: "OutBoxMessages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutBoxMessages_DeadAtUtc",
                table: "OutBoxMessages",
                column: "DeadAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutBoxMessages_DeadAtUtc",
                table: "OutBoxMessages");

            migrationBuilder.DropColumn(
                name: "DeadAtUtc",
                table: "OutBoxMessages");
        }
    }
}
