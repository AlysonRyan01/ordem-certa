using Microsoft.EntityFrameworkCore.Storage;

namespace OrdemCerta.Infrastructure.DataContext.Uow;

public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}