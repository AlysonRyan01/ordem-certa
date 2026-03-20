using OrdemCerta.Shared;

namespace OrdemCerta.Domain.Sales;

public class SaleItem
{
    public Guid Id { get; private set; }
    public Guid SaleId { get; private set; }
    public string Description { get; private set; } = null!;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice => Quantity * UnitPrice;

    protected SaleItem() { }

    private SaleItem(Guid saleId, string description, int quantity, decimal unitPrice)
    {
        Id = Guid.NewGuid();
        SaleId = saleId;
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public static Result<SaleItem> Create(Guid saleId, string description, int quantity, decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(description))
            return "A descrição do item é obrigatória.";

        if (description.Length > 300)
            return "A descrição do item deve ter no máximo 300 caracteres.";

        if (quantity <= 0)
            return "A quantidade deve ser maior que zero.";

        if (unitPrice < 0)
            return "O preço unitário não pode ser negativo.";

        return new SaleItem(saleId, description, quantity, unitPrice);
    }

    public Result Update(string description, int quantity, decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(description))
            return "A descrição do item é obrigatória.";

        if (description.Length > 300)
            return "A descrição do item deve ter no máximo 300 caracteres.";

        if (quantity <= 0)
            return "A quantidade deve ser maior que zero.";

        if (unitPrice < 0)
            return "O preço unitário não pode ser negativo.";

        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
        return Result.Success();
    }
}
