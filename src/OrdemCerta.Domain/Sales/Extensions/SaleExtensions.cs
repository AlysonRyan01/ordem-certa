using OrdemCerta.Domain.Sales.DTOs;
using OrdemCerta.Domain.ServiceOrders.Enums;

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
            sale.WarrantyDuration,
            sale.WarrantyUnit?.ToString(),
            GetWarrantyFormatted(sale.WarrantyDuration, sale.WarrantyUnit),
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

    private static string? GetWarrantyFormatted(int? duration, WarrantyUnit? unit)
    {
        if (duration is null || unit is null) return null;

        var label = unit switch
        {
            WarrantyUnit.Days   => duration == 1 ? "dia"  : "dias",
            WarrantyUnit.Months => duration == 1 ? "mês"  : "meses",
            WarrantyUnit.Years  => duration == 1 ? "ano"  : "anos",
            _                   => unit.ToString()
        };
        return $"{duration} {label}";
    }
}
