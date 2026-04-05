using OrdemCerta.Domain.ServiceOrders;

namespace OrdemCerta.Infrastructure.Repositories.ServiceOrderNotificationRepository;

public interface IServiceOrderNotificationRepository
{
    Task AddAsync(ServiceOrderNotification notification, CancellationToken cancellationToken);
    Task<List<ServiceOrderNotification>> GetByOrderIdAsync(Guid serviceOrderId, CancellationToken cancellationToken);
}
