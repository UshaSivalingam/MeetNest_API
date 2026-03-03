using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeetNest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                schema: "meetnest",
                table: "Bookings",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                schema: "meetnest",
                table: "Bookings");
        }
    }
}
