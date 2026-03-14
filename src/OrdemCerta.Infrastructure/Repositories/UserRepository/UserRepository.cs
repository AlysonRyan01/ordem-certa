using Microsoft.EntityFrameworkCore;
using OrdemCerta.Domain.Users;
using OrdemCerta.Infrastructure.DataContext.Context;
using OrdemCerta.Shared;

namespace OrdemCerta.Infrastructure.Repositories.UserRepository;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDataContext _context;

    public UserRepository(ApplicationDataContext context)
    {
        _context = context;
    }

    public async Task<Result<User>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user is null)
            return "Usuário não encontrado";

        return user;
    }

    public async Task<Result<User>> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.Value == email, cancellationToken);

        if (user is null)
            return "Usuário não encontrado";

        return user;
    }

    public async Task<bool> ExistsByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken)
    {
        return await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.CompanyId == companyId, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }
}
