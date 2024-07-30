using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketMonitor.Migrations
{
    /// <inheritdoc />
    public partial class AddDCs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DatacenterId",
                table: "TrackedItems",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Datacenters",
                columns: table => new
                {
                    Name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Datacenters", x => x.Name);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TrackedItems_DatacenterId",
                table: "TrackedItems",
                column: "DatacenterId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrackedItems_Datacenters_DatacenterId",
                table: "TrackedItems",
                column: "DatacenterId",
                principalTable: "Datacenters",
                principalColumn: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrackedItems_Datacenters_DatacenterId",
                table: "TrackedItems");

            migrationBuilder.DropTable(
                name: "Datacenters");

            migrationBuilder.DropIndex(
                name: "IX_TrackedItems_DatacenterId",
                table: "TrackedItems");

            migrationBuilder.DropColumn(
                name: "DatacenterId",
                table: "TrackedItems");
        }
    }
}
