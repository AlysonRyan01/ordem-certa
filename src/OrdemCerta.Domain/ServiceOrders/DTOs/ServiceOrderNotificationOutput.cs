namespace OrdemCerta.Domain.ServiceOrders.DTOs;

public record ServiceOrderNotificationOutput(
    Guid Id,
    string Type,
    string RecipientType,
    string RecipientName,
    string Phone,
    DateTime SentAt);
