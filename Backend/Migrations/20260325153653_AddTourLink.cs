using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Windeck.Geschichtstour.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTourLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TourLink",
                table: "Tours",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TourLink",
                table: "Tours");
        }
    }
}
