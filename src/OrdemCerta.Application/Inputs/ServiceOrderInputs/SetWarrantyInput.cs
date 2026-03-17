using OrdemCerta.Domain.ServiceOrders.Enums;

namespace OrdemCerta.Application.Inputs.ServiceOrderInputs;

public record SetWarrantyInput(int Duration, WarrantyUnit Unit);
