namespace OrdemCerta.Infrastructure.Repositories.ServiceOrderRepository;

public interface ICompanyOrderSequenceRepository
{
    Task<int> GetNextNumberAsync(Guid companyId, CancellationToken cancellationToken);
}
