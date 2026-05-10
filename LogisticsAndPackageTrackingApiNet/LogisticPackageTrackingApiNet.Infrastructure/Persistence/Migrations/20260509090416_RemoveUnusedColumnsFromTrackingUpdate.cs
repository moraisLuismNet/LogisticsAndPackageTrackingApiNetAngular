using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogisticPackageTrackingApiNet.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedColumnsFromTrackingUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TrackingUpdates");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TrackingUpdates");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "TrackingUpdates");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "TrackingUpdates");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TrackingUpdates");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TrackingUpdates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TrackingUpdates",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TrackingUpdates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "TrackingUpdates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "TrackingUpdates",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TrackingUpdates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TrackingUpdates",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
