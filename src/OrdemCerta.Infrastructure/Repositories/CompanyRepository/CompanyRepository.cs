using Microsoft.EntityFrameworkCore;
using OrdemCerta.Domain.Companies;
using OrdemCerta.Infrastructure.DataContext.Context;
using OrdemCerta.Shared;

namespace OrdemCerta.Infrastructure.Repositories.CompanyRepository;

public class CompanyRepository : ICompanyRepository
{
    private readonly ApplicationDataContext _context;

    public CompanyRepository(ApplicationDataContext context)
    {
        _context = context;
    }

    public async Task<Result<Company>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var company = await _context.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (company is null)
            return "Empresa não encontrada";

        return company;
    }

    public async Task AddAsync(Company company, CancellationToken cancellationToken)
    {
        await _context.Companies.AddAsync(company, cancellationToken);
    }

    public Task UpdateAsync(Company company, CancellationToken cancellationToken)
    {
        _context.Companies.Update(company);
        return Task.CompletedTask;
    }
}
