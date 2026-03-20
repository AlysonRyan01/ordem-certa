using OrdemCerta.Domain.MarketingProspects;

namespace OrdemCerta.Infrastructure.Repositories.MarketingProspectRepository;

public interface IMarketingProspectRepository
{
    Task<HashSet<string>> GetAllPlaceIdsAsync(CancellationToken cancellationToken);
    Task<List<MarketingProspect>> GetPendingAsync(int limit, CancellationToken cancellationToken);
    Task AddAsync(MarketingProspect prospect, CancellationToken cancellationToken);
    Task UpdateAsync(MarketingProspect prospect, CancellationToken cancellationToken);
}
