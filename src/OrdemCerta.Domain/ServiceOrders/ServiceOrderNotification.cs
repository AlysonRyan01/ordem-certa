using OrdemCerta.Domain.ServiceOrders.Enums;
using OrdemCerta.Shared;

namespace OrdemCerta.Domain.ServiceOrders;

public class ServiceOrderNotification : Entity
{
    public Guid ServiceOrderId { get; private set; }
    public Guid CompanyId { get; private set; }
    public NotificationType Type { get; private set; }
    public NotificationRecipientType RecipientType { get; private set; }
    public string RecipientName { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public DateTime SentAt { get; private set; }

    protected ServiceOrderNotification() { }

    public static ServiceOrderNotification Create(
        Guid serviceOrderId,
        Guid companyId,
        NotificationType type,
        NotificationRecipientType recipientType,
        string recipientName,
        string phone)
    {
        return new ServiceOrderNotification
        {
            Id = Guid.NewGuid(),
            ServiceOrderId = serviceOrderId,
            CompanyId = companyId,
            Type = type,
            RecipientType = recipientType,
            RecipientName = recipientName,
            Phone = phone,
            SentAt = DateTime.UtcNow,
        };
    }
}
