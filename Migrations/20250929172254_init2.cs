using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PegasusBackend.Migrations
{
    /// <inheritdoc />
    public partial class init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PickUpCoordinate",
                table: "Bookings",
                newName: "PickUpLongitude");

            migrationBuilder.RenameColumn(
                name: "DropOffCoordinate",
                table: "Bookings",
                newName: "PickUpLatitude");

            migrationBuilder.AddColumn<double>(
                name: "DropOffLatitude",
                table: "Bookings",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DropOffLongitude",
                table: "Bookings",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DropOffLatitude",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "DropOffLongitude",
                table: "Bookings");

            migrationBuilder.RenameColumn(
                name: "PickUpLongitude",
                table: "Bookings",
                newName: "PickUpCoordinate");

            migrationBuilder.RenameColumn(
                name: "PickUpLatitude",
                table: "Bookings",
                newName: "DropOffCoordinate");
        }
    }
}
