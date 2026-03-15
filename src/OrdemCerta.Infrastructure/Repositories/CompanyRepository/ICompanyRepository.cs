using OrdemCerta.Domain.Companies;
using OrdemCerta.Shared;

namespace OrdemCerta.Infrastructure.Repositories.CompanyRepository;

public interface ICompanyRepository
{
    Task<Result<Company>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<Company>> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task AddAsync(Company company, CancellationToken cancellationToken);
    Task UpdateAsync(Company company, CancellationToken cancellationToken);
}
