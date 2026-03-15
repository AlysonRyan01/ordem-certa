using MediatR;

namespace OrdemCerta.Domain.ServiceOrders.Events;

public record BudgetCreatedEvent(
    Guid ServiceOrderId,
    int OrderNumber,
    Guid CompanyId,
    Guid CustomerId,
    string DeviceType,
    string Brand,
    string Model,
    decimal BudgetValue,
    string BudgetDescription) : INotification;
