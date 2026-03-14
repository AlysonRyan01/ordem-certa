using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrdemCerta.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ServiceOrderMap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "service_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    reported_defect = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    accessories = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    observations = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    entry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    technician_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_orders", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_service_orders_company_id",
                table: "service_orders",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_service_orders_customer_id",
                table: "service_orders",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_service_orders_status",
                table: "service_orders",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "service_orders");
        }
    }
}
