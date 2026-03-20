using OrdemCerta.Domain.Sales.Enums;

namespace OrdemCerta.Application.Inputs.SaleInputs;

public record UpdateSaleInput(
    Guid? CustomerId,
    string? CustomerName,
    string? Description,
    SalePaymentMethod PaymentMethod,
    string? Notes
);
