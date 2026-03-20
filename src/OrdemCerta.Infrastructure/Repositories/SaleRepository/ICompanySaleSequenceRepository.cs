namespace OrdemCerta.Infrastructure.Repositories.SaleRepository;

public interface ICompanySaleSequenceRepository
{
    Task<int> GetNextNumberAsync(Guid companyId, CancellationToken cancellationToken);
}
