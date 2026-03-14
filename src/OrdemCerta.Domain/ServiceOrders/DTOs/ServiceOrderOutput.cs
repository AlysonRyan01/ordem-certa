namespace OrdemCerta.Domain.ServiceOrders.DTOs;

public record ServiceOrderOutput(
    Guid Id,
    Guid CompanyId,
    Guid CustomerId,
    string DeviceType,
    string Brand,
    string Model,
    string ReportedDefect,
    string? Accessories,
    string? Observations,
    string Status,
    DateTime EntryDate,
    string? TechnicianName);
