using OrdemCerta.Domain.Sales.Enums;

namespace OrdemCerta.Domain.Sales.DTOs;

public record SaleOutput(
    Guid Id,
    Guid CompanyId,
    Guid? CustomerId,
    int SaleNumber,
    string? CustomerName,
    string? Description,
    SaleStatus Status,
    SalePaymentMethod PaymentMethod,
    decimal TotalValue,
    int? WarrantyDuration,
    string? WarrantyUnit,
    string? WarrantyFormatted,
    string? Notes,
    DateTime SaleDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyCollection<SaleItemOutput> Items
);

public record SaleItemOutput(
    Guid Id,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);
