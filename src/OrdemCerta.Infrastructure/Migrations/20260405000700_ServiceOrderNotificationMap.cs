using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrdemCerta.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ServiceOrderNotificationMap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "service_order_notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    recipient_type = table.Column<int>(type: "integer", nullable: false),
                    recipient_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_order_notifications", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_service_order_notifications_company_id",
                table: "service_order_notifications",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_service_order_notifications_service_order_id",
                table: "service_order_notifications",
                column: "service_order_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "service_order_notifications");
        }
    }
}
