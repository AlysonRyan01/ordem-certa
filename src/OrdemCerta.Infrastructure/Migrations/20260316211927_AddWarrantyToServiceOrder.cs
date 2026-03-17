using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrdemCerta.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWarrantyToServiceOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "warranty_duration",
                table: "service_orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "warranty_unit",
                table: "service_orders",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "warranty_duration",
                table: "service_orders");

            migrationBuilder.DropColumn(
                name: "warranty_unit",
                table: "service_orders");
        }
    }
}
