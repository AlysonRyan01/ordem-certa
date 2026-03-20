namespace OrdemCerta.Application.Inputs.SaleInputs;

public record UpdateSaleItemInput(
    string Description,
    int Quantity,
    decimal UnitPrice
);
