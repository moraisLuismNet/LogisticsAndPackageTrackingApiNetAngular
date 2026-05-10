using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogisticPackageTrackingApiNet.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationCoordinatesToShipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DestinationLatitude",
                table: "Shipments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DestinationLongitude",
                table: "Shipments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "OriginLatitude",
                table: "Shipments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "OriginLongitude",
                table: "Shipments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DestinationLatitude",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "DestinationLongitude",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "OriginLatitude",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "OriginLongitude",
                table: "Shipments");
        }
    }
}
