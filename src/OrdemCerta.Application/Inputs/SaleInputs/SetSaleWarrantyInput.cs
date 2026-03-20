using OrdemCerta.Domain.ServiceOrders.Enums;

namespace OrdemCerta.Application.Inputs.SaleInputs;

public record SetSaleWarrantyInput(
    int Duration,
    WarrantyUnit Unit
);
