using OrdemCerta.Domain.Users;
using OrdemCerta.Shared;

namespace OrdemCerta.Infrastructure.Repositories.UserRepository;

public interface IUserRepository
{
    Task<Result<User>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<User>> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<bool> ExistsByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken);
    Task AddAsync(User user, CancellationToken cancellationToken);
    Task UpdateAsync(User user, CancellationToken cancellationToken);
}
