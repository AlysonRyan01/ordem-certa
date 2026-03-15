using Microsoft.EntityFrameworkCore;
using OrdemCerta.Domain.ServiceOrders;
using OrdemCerta.Infrastructure.DataContext.Context;

namespace OrdemCerta.Infrastructure.Repositories.ServiceOrderRepository;

public class CompanyOrderSequenceRepository : ICompanyOrderSequenceRepository
{
    private readonly ApplicationDataContext _context;

    public CompanyOrderSequenceRepository(ApplicationDataContext context)
    {
        _context = context;
    }

    public async Task<int> GetNextNumberAsync(Guid companyId, CancellationToken cancellationToken)
    {
        var sequence = await _context.CompanyOrderSequences
            .FirstOrDefaultAsync(s => s.CompanyId == companyId, cancellationToken);

        if (sequence is null)
        {
            sequence = CompanyOrderSequence.Create(companyId);
            _context.CompanyOrderSequences.Add(sequence);
        }

        return sequence.Increment();
    }
}
