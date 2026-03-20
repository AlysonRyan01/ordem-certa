using Microsoft.EntityFrameworkCore;
using OrdemCerta.Domain.Companies.Interfaces;
using OrdemCerta.Domain.Sales;
using OrdemCerta.Domain.Sales.Enums;
using OrdemCerta.Infrastructure.DataContext.Context;
using OrdemCerta.Shared;

namespace OrdemCerta.Infrastructure.Repositories.SaleRepository;

public class SaleRepository : ISaleRepository
{
    private readonly ApplicationDataContext _context;
    private readonly ICurrentCompany _currentCompany;

    public SaleRepository(ApplicationDataContext context, ICurrentCompany currentCompany)
    {
        _context = context;
        _currentCompany = currentCompany;
    }

    public async Task<Result<Sale>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var sale = await _context.Sales
            .Include(s => s.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.CompanyId == _currentCompany.CompanyId, cancellationToken);

        if (sale is null)
            return "Venda não encontrada.";

        return sale;
    }

    public async Task<Result<Sale>> GetByIdTrackedAsync(Guid id, CancellationToken cancellationToken)
    {
        var sale = await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id && s.CompanyId == _currentCompany.CompanyId, cancellationToken);

        if (sale is null)
            return "Venda não encontrada.";

        return sale;
    }

    public async Task<List<Sale>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        if (page <= 0) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        return await _context.Sales
            .Include(s => s.Items)
            .Where(s => s.CompanyId == _currentCompany.CompanyId)
            .AsNoTracking()
            .OrderByDescending(s => s.SaleDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Sale>> GetByStatusAsync(SaleStatus status, int page, int pageSize, CancellationToken cancellationToken)
    {
        if (page <= 0) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        return await _context.Sales
            .Include(s => s.Items)
            .Where(s => s.CompanyId == _currentCompany.CompanyId && s.Status == status)
            .AsNoTracking()
            .OrderByDescending(s => s.SaleDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Sale>> GetByCustomerAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken)
    {
        if (page <= 0) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        return await _context.Sales
            .Include(s => s.Items)
            .Where(s => s.CompanyId == _currentCompany.CompanyId && s.CustomerId == customerId)
            .AsNoTracking()
            .OrderByDescending(s => s.SaleDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken)
    {
        return await _context.Sales
            .AsNoTracking()
            .CountAsync(s => s.CompanyId == _currentCompany.CompanyId, cancellationToken);
    }

    public async Task AddAsync(Sale sale, CancellationToken cancellationToken)
    {
        await _context.Sales.AddAsync(sale, cancellationToken);
    }

    public Task UpdateAsync(Sale sale, CancellationToken cancellationToken)
    {
        _context.Sales.Update(sale);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Sale sale, CancellationToken cancellationToken)
    {
        _context.Sales.Remove(sale);
        return Task.CompletedTask;
    }
}
