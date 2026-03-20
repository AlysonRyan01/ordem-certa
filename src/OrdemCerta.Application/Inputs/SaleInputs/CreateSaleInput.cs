using OrdemCerta.Domain.Sales.Enums;

namespace OrdemCerta.Application.Inputs.SaleInputs;

public record CreateSaleInput(
    Guid? CustomerId,
    string? CustomerName,
    string? Description,
    SalePaymentMethod PaymentMethod,
    string? Notes,
    List<SaleItemInput> Items
);

public record SaleItemInput(
    string Description,
    int Quantity,
    decimal UnitPrice
);
