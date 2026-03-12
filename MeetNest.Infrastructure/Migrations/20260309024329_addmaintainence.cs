using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeetNest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addmaintainence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UnderMaintenance",
                schema: "meetnest",
                table: "Rooms",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnderMaintenance",
                schema: "meetnest",
                table: "Rooms");
        }
    }
}
