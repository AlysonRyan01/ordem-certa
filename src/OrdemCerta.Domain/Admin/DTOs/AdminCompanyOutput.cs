namespace OrdemCerta.Domain.Admin.DTOs;

public record AdminCompanyOutput(
    Guid Id,
    string Name,
    string Email,
    string Plan,
    bool HasActiveSubscription,
    int ServiceOrderCount,
    DateTime CreatedAt);
