using OrdemCerta.Domain.Sales.DTOs;

namespace OrdemCerta.Domain.Sales.Extensions;

public static class SaleExtensions
{
    public static SaleOutput ToOutput(this Sale sale)
    {
        return new SaleOutput(
            sale.Id,
            sale.CompanyId,
            sale.CustomerId,
            sale.SaleNumber,
            sale.CustomerName,
            sale.Description,
            sale.Status,
            sale.PaymentMethod,
            sale.TotalValue,
            sale.Warranty?.Duration,
            sale.Warranty?.Unit.ToString(),
            sale.Warranty?.GetFormatted(),
            sale.Notes,
            sale.SaleDate,
            sale.CreatedAt,
            sale.UpdatedAt,
            sale.Items.Select(i => i.ToItemOutput()).ToList().AsReadOnly()
        );
    }

    public static SaleItemOutput ToItemOutput(this SaleItem item)
    {
        return new SaleItemOutput(
            item.Id,
            item.Description,
            item.Quantity,
            item.UnitPrice,
            item.TotalPrice
        );
    }

    public static List<SaleOutput> ToOutputList(this IEnumerable<Sale> sales)
        => sales.Select(s => s.ToOutput()).ToList();
}
