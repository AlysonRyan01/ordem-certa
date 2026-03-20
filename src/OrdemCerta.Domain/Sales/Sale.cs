using OrdemCerta.Domain.Sales.Enums;
using OrdemCerta.Domain.ServiceOrders.ValueObjects;
using OrdemCerta.Shared;

namespace OrdemCerta.Domain.Sales;

public class Sale : AggregateRoot
{
    public Guid CompanyId { get; private set; }
    public Guid? CustomerId { get; private set; }
    public int SaleNumber { get; private set; }
    public string? CustomerName { get; private set; }
    public string? Description { get; private set; }
    public SaleStatus Status { get; private set; }
    public SalePaymentMethod PaymentMethod { get; private set; }
    public decimal TotalValue { get; private set; }
    public Warranty? Warranty { get; private set; }
    public string? Notes { get; private set; }
    public DateTime SaleDate { get; private set; }

    private readonly List<SaleItem> _items = [];
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    protected Sale() { }

    private Sale(Guid companyId, Guid? customerId, int saleNumber, string? customerName, string? description, SalePaymentMethod paymentMethod, string? notes)
    {
        Id = Guid.NewGuid();
        CompanyId = companyId;
        CustomerId = customerId;
        SaleNumber = saleNumber;
        CustomerName = customerName;
        Description = description;
        Status = SaleStatus.Pending;
        PaymentMethod = paymentMethod;
        TotalValue = 0;
        Notes = notes;
        SaleDate = DateTime.UtcNow;
    }

    public static Result<Sale> Create(Guid companyId, Guid? customerId, int saleNumber, string? customerName, string? description, SalePaymentMethod paymentMethod, string? notes)
    {
        if (customerId is null && string.IsNullOrWhiteSpace(customerName))
            return "Informe o cliente ou o nome do cliente avulso.";

        return new Sale(companyId, customerId, saleNumber, customerName, description, paymentMethod, notes);
    }

    public Result AddItem(string description, int quantity, decimal unitPrice)
    {
        var itemResult = SaleItem.Create(Id, description, quantity, unitPrice);
        if (itemResult.IsFailure)
            return Result.Failure(itemResult.Errors);

        _items.Add(itemResult.Value!);
        RecalculateTotal();
        return Result.Success();
    }

    public Result UpdateItem(Guid itemId, string description, int quantity, decimal unitPrice)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
            return "Item não encontrado.";

        var updateResult = item.Update(description, quantity, unitPrice);
        if (updateResult.IsFailure)
            return updateResult;

        RecalculateTotal();
        return Result.Success();
    }

    public Result RemoveItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
            return "Item não encontrado.";

        _items.Remove(item);
        RecalculateTotal();
        return Result.Success();
    }

    public Result Update(Guid? customerId, string? customerName, string? description, SalePaymentMethod paymentMethod, string? notes)
    {
        if (Status == SaleStatus.Cancelled)
            return "Não é possível alterar uma venda cancelada.";

        if (customerId is null && string.IsNullOrWhiteSpace(customerName))
            return "Informe o cliente ou o nome do cliente avulso.";

        CustomerId = customerId;
        CustomerName = customerName;
        Description = description;
        PaymentMethod = paymentMethod;
        Notes = notes;
        return Result.Success();
    }

    public Result Complete()
    {
        if (Status == SaleStatus.Completed)
            return "A venda já está concluída.";

        if (Status == SaleStatus.Cancelled)
            return "Não é possível concluir uma venda cancelada.";

        if (!_items.Any())
            return "A venda deve ter pelo menos um item.";

        Status = SaleStatus.Completed;
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == SaleStatus.Cancelled)
            return "A venda já está cancelada.";

        Status = SaleStatus.Cancelled;
        return Result.Success();
    }

    public Result SetWarranty(Warranty warranty)
    {
        if (Status == SaleStatus.Cancelled)
            return "Não é possível definir garantia para uma venda cancelada.";

        Warranty = warranty;
        return Result.Success();
    }

    private void RecalculateTotal()
    {
        TotalValue = _items.Sum(i => i.TotalPrice);
    }
}
