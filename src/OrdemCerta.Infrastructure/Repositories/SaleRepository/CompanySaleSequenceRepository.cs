using Microsoft.EntityFrameworkCore;
using OrdemCerta.Domain.Sales;
using OrdemCerta.Infrastructure.DataContext.Context;

namespace OrdemCerta.Infrastructure.Repositories.SaleRepository;

public class CompanySaleSequenceRepository : ICompanySaleSequenceRepository
{
    private readonly ApplicationDataContext _context;

    public CompanySaleSequenceRepository(ApplicationDataContext context)
    {
        _context = context;
    }

    public async Task<int> GetNextNumberAsync(Guid companyId, CancellationToken cancellationToken)
    {
        var sequence = await _context.CompanySaleSequences
            .FirstOrDefaultAsync(s => s.CompanyId == companyId, cancellationToken);

        if (sequence is null)
        {
            sequence = CompanySaleSequence.Create(companyId);
            await _context.CompanySaleSequences.AddAsync(sequence, cancellationToken);
        }

        return sequence.Increment();
    }
}
