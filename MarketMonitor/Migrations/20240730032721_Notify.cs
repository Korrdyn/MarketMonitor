using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketMonitor.Migrations
{
    /// <inheritdoc />
    public partial class Notify : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "NotifyFreq",
                table: "Users",
                type: "time(6)",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastNotify",
                table: "TrackedItems",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotifyFreq",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastNotify",
                table: "TrackedItems");
        }
    }
}
