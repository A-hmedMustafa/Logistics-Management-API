using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logis.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceOutboxMessageId1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SourceOutboxMessageId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_SourceOutboxMessageId",
                table: "Notifications",
                column: "SourceOutboxMessageId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notifications_SourceOutboxMessageId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "SourceOutboxMessageId",
                table: "Notifications");
        }
    }
}
