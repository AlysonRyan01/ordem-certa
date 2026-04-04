using Microsoft.EntityFrameworkCore;
using OrdemCerta.Domain.MarketingProspects;
using OrdemCerta.Domain.MarketingProspects.Enums;
using OrdemCerta.Infrastructure.DataContext.Context;

namespace OrdemCerta.Infrastructure.Repositories.MarketingProspectRepository;

public class MarketingProspectRepository(ApplicationDataContext context) : IMarketingProspectRepository
{
    public async Task<HashSet<string>> GetAllPlaceIdsAsync(CancellationToken cancellationToken)
    {
        var ids = await context.MarketingProspects
            .AsNoTracking()
            .Select(p => p.PlaceId)
            .ToListAsync(cancellationToken);

        return [..ids];
    }

    public async Task<List<MarketingProspect>> GetPendingAsync(int limit, CancellationToken cancellationToken)
    {
        var pending = await context.MarketingProspects
            .Where(p => p.Status == ProspectStatus.Pending)
            .OrderBy(p => p.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        if (pending.Count < limit)
        {
            var failed = await context.MarketingProspects
                .Where(p => p.Status == ProspectStatus.Failed)
                .OrderBy(p => p.CreatedAt)
                .Take(limit - pending.Count)
                .ToListAsync(cancellationToken);

            pending.AddRange(failed);
        }

        return pending;
    }

    public async Task AddAsync(MarketingProspect prospect, CancellationToken cancellationToken)
    {
        await context.MarketingProspects.AddAsync(prospect, cancellationToken);
    }

    public Task UpdateAsync(MarketingProspect prospect, CancellationToken cancellationToken)
    {
        context.MarketingProspects.Update(prospect);
        return Task.CompletedTask;
    }
}
