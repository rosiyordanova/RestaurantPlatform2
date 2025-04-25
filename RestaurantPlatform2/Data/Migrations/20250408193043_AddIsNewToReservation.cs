using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantPlatform2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsNewToReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsNew",
                table: "Reservations",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsNew",
                table: "Reservations");
        }
    }
}
