using Microsoft.EntityFrameworkCore.Storage;
using OrdemCerta.Infrastructure.DataContext.Context;

namespace OrdemCerta.Infrastructure.DataContext.Uow;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDataContext _context;

    public UnitOfWork(ApplicationDataContext context)
    {
        _context = context;
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            foreach (var entry in _context.ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case Microsoft.EntityFrameworkCore.EntityState.Added:
                        entry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                        break;
                    case Microsoft.EntityFrameworkCore.EntityState.Modified:
                    case Microsoft.EntityFrameworkCore.EntityState.Deleted:
                        entry.Reload();
                        break;
                }
            }
        }, cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Database.BeginTransactionAsync(cancellationToken);
    }
}