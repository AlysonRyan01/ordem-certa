using Microsoft.EntityFrameworkCore;
using OrdemCerta.Domain.ServiceOrders;
using OrdemCerta.Infrastructure.DataContext.Context;

namespace OrdemCerta.Infrastructure.Repositories.ServiceOrderNotificationRepository;

public class ServiceOrderNotificationRepository(ApplicationDataContext context) : IServiceOrderNotificationRepository
{
    public async Task AddAsync(ServiceOrderNotification notification, CancellationToken cancellationToken)
    {
        await context.ServiceOrderNotifications.AddAsync(notification, cancellationToken);
    }

    public async Task<List<ServiceOrderNotification>> GetByOrderIdAsync(Guid serviceOrderId, CancellationToken cancellationToken)
    {
        return await context.ServiceOrderNotifications
            .AsNoTracking()
            .Where(n => n.ServiceOrderId == serviceOrderId)
            .OrderBy(n => n.SentAt)
            .ToListAsync(cancellationToken);
    }
}
