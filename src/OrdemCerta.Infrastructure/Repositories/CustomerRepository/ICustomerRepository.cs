using OrdemCerta.Domain.Customers;
using OrdemCerta.Shared;

namespace OrdemCerta.Infrastructure.Repositories.CustomerRepository;

public interface ICustomerRepository
{
    Task<Result<Customer>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<Customer>> GetByIdTrackedAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<Customer>>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<Customer>>> GetByNameAsync(string searchTerm, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, string>> GetNamesByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
    Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default);
    Task DeleteAsync(Customer customer, CancellationToken cancellationToken = default);
}