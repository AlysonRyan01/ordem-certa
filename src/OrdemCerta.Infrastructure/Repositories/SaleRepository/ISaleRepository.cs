using OrdemCerta.Domain.Sales;
using OrdemCerta.Domain.Sales.Enums;
using OrdemCerta.Shared;

namespace OrdemCerta.Infrastructure.Repositories.SaleRepository;

public interface ISaleRepository
{
    Task<Result<Sale>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<Sale>> GetByIdTrackedAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Sale>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<List<Sale>> GetByStatusAsync(SaleStatus status, int page, int pageSize, CancellationToken cancellationToken);
    Task<List<Sale>> GetByCustomerAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken);
    Task<int> CountAsync(CancellationToken cancellationToken);
    Task AddAsync(Sale sale, CancellationToken cancellationToken);
    Task UpdateAsync(Sale sale, CancellationToken cancellationToken);
    Task DeleteAsync(Sale sale, CancellationToken cancellationToken);
}
