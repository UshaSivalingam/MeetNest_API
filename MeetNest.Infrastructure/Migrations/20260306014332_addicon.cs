using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeetNest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addicon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Icon",
                schema: "meetnest",
                table: "Facilities",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Icon",
                schema: "meetnest",
                table: "Facilities");
        }
    }
}
