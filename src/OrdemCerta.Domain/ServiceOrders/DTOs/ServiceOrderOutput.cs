namespace OrdemCerta.Domain.ServiceOrders.DTOs;

public record ServiceOrderOutput(
    Guid Id,
    Guid CompanyId,
    Guid CustomerId,
    int OrderNumber,
    string DeviceType,
    string Brand,
    string Model,
    string ReportedDefect,
    string? Accessories,
    string? Observations,
    string Status,
    DateTime EntryDate,
    string? TechnicianName,
    decimal? BudgetValue,
    string? BudgetDescription,
    string? CompanyName = null);
