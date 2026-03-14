using OrdemCerta.Domain.ServiceOrders.Enums;

namespace OrdemCerta.Application.Inputs.ServiceOrderInputs;

public record ChangeStatusInput(ServiceOrderStatus Status);
