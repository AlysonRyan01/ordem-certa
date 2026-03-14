using OrdemCerta.Domain.ServiceOrders.DTOs;

namespace OrdemCerta.Domain.ServiceOrders.Extensions;

public static class ServiceOrderExtensions
{
    public static ServiceOrderOutput ToOutput(this ServiceOrder order)
    {
        return new ServiceOrderOutput(
            order.Id,
            order.CompanyId,
            order.CustomerId,
            order.Equipment.DeviceType,
            order.Equipment.Brand,
            order.Equipment.Model,
            order.Equipment.ReportedDefect,
            order.Equipment.Accessories,
            order.Equipment.Observations,
            order.Status.ToString(),
            order.EntryDate,
            order.TechnicianName);
    }

    public static IEnumerable<ServiceOrderOutput> ToOutput(this IEnumerable<ServiceOrder> orders)
        => orders.Select(o => o.ToOutput());
}
