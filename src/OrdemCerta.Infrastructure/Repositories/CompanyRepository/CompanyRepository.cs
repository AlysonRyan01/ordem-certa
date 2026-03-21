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

    public async Task<Result<Company>> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var company = await _context.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Email == email, cancellationToken);

        if (company is null)
            return "E-mail ou senha inválidos";

        return company;
    }

    public async Task<Result<Company>> GetByStripeCustomerIdAsync(string stripeCustomerId, CancellationToken cancellationToken)
    {
        var company = await _context.Companies
            .FirstOrDefaultAsync(c => c.StripeCustomerId == stripeCustomerId, cancellationToken);

        if (company is null)
            return "Empresa não encontrada";

        return company;
    }

    public async Task<Result<Company>> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var company = await _context.Companies
            .FirstOrDefaultAsync(c => c.RefreshToken == refreshToken, cancellationToken);

        if (company is null)
            return "Refresh token inválido";

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
