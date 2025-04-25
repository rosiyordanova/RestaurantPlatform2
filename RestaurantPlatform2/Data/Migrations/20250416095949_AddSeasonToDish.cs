using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantPlatform2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSeasonToDish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Season",
                table: "Dishes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Season",
                table: "Dishes");
        }
    }
}
