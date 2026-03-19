using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrdemCerta.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ServiceOrderRepairStatusMap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "budget_status",
                table: "service_orders",
                type: "integer",
                nullable: true);

            // Migra valores do status (do highest para o lowest para evitar conflitos)
            // Cancelled: 10 → 6
            migrationBuilder.Sql("UPDATE service_orders SET status = 6 WHERE status = 10");
            // Delivered: 9 → 5
            migrationBuilder.Sql("UPDATE service_orders SET status = 5 WHERE status = 9");
            // ReadyForPickup: 8 → 4
            migrationBuilder.Sql("UPDATE service_orders SET status = 4 WHERE status = 8");
            // UnderRepair: 7 → 3
            migrationBuilder.Sql("UPDATE service_orders SET status = 3 WHERE status = 7");
            // BudgetRefused: 6 → BudgetPending(2) + Disapproved(3)
            migrationBuilder.Sql("UPDATE service_orders SET status = 2, budget_status = 3 WHERE status = 6");
            // BudgetApproved: 5 → BudgetPending(2) + Approved(2)
            migrationBuilder.Sql("UPDATE service_orders SET status = 2, budget_status = 2 WHERE status = 5");
            // WaitingApproval: 4 → BudgetPending(2) + Waiting(4)
            migrationBuilder.Sql("UPDATE service_orders SET status = 2, budget_status = 4 WHERE status = 4");
            // BudgetPending: 3 → BudgetPending(2) + Entered(1)
            migrationBuilder.Sql("UPDATE service_orders SET status = 2, budget_status = 1 WHERE status = 3");
            // UnderAnalysis: 2 → 1
            migrationBuilder.Sql("UPDATE service_orders SET status = 1 WHERE status = 2");
            // Received: 1 → UnderAnalysis(1) — sem alteração numérica
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "budget_status",
                table: "service_orders");
        }
    }
}
