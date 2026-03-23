using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeetNest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addblock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BlockFromDate",
                schema: "meetnest",
                table: "Rooms",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BlockReason",
                schema: "meetnest",
                table: "Rooms",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlockFromDate",
                schema: "meetnest",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "BlockReason",
                schema: "meetnest",
                table: "Rooms");
        }
    }
}
