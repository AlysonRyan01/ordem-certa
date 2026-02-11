using OrdemCerta.Domain.Customers;
using OrdemCerta.Shared;

namespace OrdemCerta.Infrastructure.Repositories.CustomerRepository;

public interface ICustomerRepository
{
    Task<Result<Customer>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<Customer>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Result<Customer>> GetByDocumentAsync(string document, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<Customer>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<Customer>>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
    Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default);
    Task DeleteAsync(Customer customer, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByDocumentAsync(string document, CancellationToken cancellationToken = default);

}