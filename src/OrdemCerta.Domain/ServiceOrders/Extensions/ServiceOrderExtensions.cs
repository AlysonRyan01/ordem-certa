using OrdemCerta.Domain.ServiceOrders.DTOs;

namespace OrdemCerta.Domain.ServiceOrders.Extensions;

public static class ServiceOrderExtensions
{
    public static ServiceOrderOutput ToOutput(this ServiceOrder order, string? companyName = null, string? customerName = null)
    {
        return new ServiceOrderOutput(
            order.Id,
            order.CompanyId,
            order.CustomerId,
            order.OrderNumber,
            order.DeviceType,
            order.Brand,
            order.Model,
            order.ReportedDefect,
            order.Accessories,
            order.Observations,
            order.Status.ToString(),
            order.BudgetStatus?.ToString(),
            order.RepairResult?.ToString(),
            order.WarrantyDuration,
            order.WarrantyUnit?.ToString(),
            order.EntryDate,
            order.TechnicianName,
            order.BudgetValue,
            order.BudgetDescription,
            companyName,
            customerName);
    }

    public static IEnumerable<ServiceOrderOutput> ToOutput(this IEnumerable<ServiceOrder> orders)
        => orders.Select(o => o.ToOutput());
}
