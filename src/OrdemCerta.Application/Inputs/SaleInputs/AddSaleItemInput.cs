namespace OrdemCerta.Application.Inputs.SaleInputs;

public record AddSaleItemInput(
    string Description,
    int Quantity,
    decimal UnitPrice
);
