using MediatR;

namespace OrdemCerta.Domain.ServiceOrders.Events;

public record ServiceReadyEvent(
    Guid ServiceOrderId,
    int OrderNumber,
    Guid CompanyId,
    Guid CustomerId,
    string DeviceType,
    string Brand,
    string Model) : INotification;
