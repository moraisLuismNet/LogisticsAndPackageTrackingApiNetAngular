using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogisticPackageTrackingApiNet.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameSenderNameToMail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SenderName",
                table: "Shipments");

            migrationBuilder.AddColumn<string>(
                name: "Mail",
                table: "Shipments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mail",
                table: "Shipments");

            migrationBuilder.AddColumn<string>(
                name: "SenderName",
                table: "Shipments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
