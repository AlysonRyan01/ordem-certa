using OrdemCerta.Domain.ServiceOrders;
using OrdemCerta.Domain.ServiceOrders.Enums;
using OrdemCerta.Shared;

namespace OrdemCerta.Infrastructure.Repositories.ServiceOrderRepository;

public interface IServiceOrderRepository
{
    Task<Result<ServiceOrder>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<ServiceOrder>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<List<ServiceOrder>> GetByStatusAsync(ServiceOrderStatus status, int page, int pageSize, CancellationToken cancellationToken);
    Task<List<ServiceOrder>> GetByCustomerAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken);
    Task<int> CountAsync(CancellationToken cancellationToken);
    Task AddAsync(ServiceOrder order, CancellationToken cancellationToken);
    Task UpdateAsync(ServiceOrder order, CancellationToken cancellationToken);
    Task DeleteAsync(ServiceOrder order, CancellationToken cancellationToken);
}
