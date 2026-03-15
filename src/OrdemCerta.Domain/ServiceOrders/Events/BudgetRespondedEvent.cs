using MediatR;

namespace OrdemCerta.Domain.ServiceOrders.Events;

public record BudgetRespondedEvent(
    Guid ServiceOrderId,
    int OrderNumber,
    Guid CompanyId,
    bool Approved) : INotification;
