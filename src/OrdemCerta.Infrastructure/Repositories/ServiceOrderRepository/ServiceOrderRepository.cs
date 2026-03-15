using Microsoft.EntityFrameworkCore;
using OrdemCerta.Domain.Companies.Interfaces;
using OrdemCerta.Domain.ServiceOrders;
using OrdemCerta.Domain.ServiceOrders.Enums;
using OrdemCerta.Infrastructure.DataContext.Context;
using OrdemCerta.Shared;

namespace OrdemCerta.Infrastructure.Repositories.ServiceOrderRepository;

public class ServiceOrderRepository : IServiceOrderRepository
{
    private readonly ApplicationDataContext _context;
    private readonly ICurrentCompany _currentCompany;

    public ServiceOrderRepository(ApplicationDataContext context, ICurrentCompany currentCompany)
    {
        _context = context;
        _currentCompany = currentCompany;
    }

    public async Task<Result<ServiceOrder>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await _context.ServiceOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id && o.CompanyId == _currentCompany.CompanyId, cancellationToken);

        if (order is null)
            return "Ordem de serviço não encontrada";

        return order;
    }

    public async Task<List<ServiceOrder>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        if (page <= 0) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        return await _context.ServiceOrders
            .Where(o => o.CompanyId == _currentCompany.CompanyId)
            .AsNoTracking()
            .OrderByDescending(o => o.EntryDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ServiceOrder>> GetByStatusAsync(ServiceOrderStatus status, int page, int pageSize, CancellationToken cancellationToken)
    {
        if (page <= 0) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        return await _context.ServiceOrders
            .Where(o => o.CompanyId == _currentCompany.CompanyId && o.Status == status)
            .AsNoTracking()
            .OrderByDescending(o => o.EntryDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ServiceOrder>> GetByCustomerAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken)
    {
        if (page <= 0) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        return await _context.ServiceOrders
            .Where(o => o.CompanyId == _currentCompany.CompanyId && o.CustomerId == customerId)
            .AsNoTracking()
            .OrderByDescending(o => o.EntryDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken)
    {
        return await _context.ServiceOrders
            .AsNoTracking()
            .CountAsync(o => o.CompanyId == _currentCompany.CompanyId, cancellationToken);
    }

    public async Task<int> CountByStatusesAsync(ServiceOrderStatus[] statuses, CancellationToken cancellationToken)
    {
        return await _context.ServiceOrders
            .AsNoTracking()
            .CountAsync(o => o.CompanyId == _currentCompany.CompanyId && statuses.Contains(o.Status), cancellationToken);
    }

    public async Task<List<ServiceOrder>> GetRecentAsync(int count, CancellationToken cancellationToken)
    {
        return await _context.ServiceOrders
            .Where(o => o.CompanyId == _currentCompany.CompanyId)
            .AsNoTracking()
            .OrderByDescending(o => o.EntryDate)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<(ServiceOrderStatus Status, int Count)>> GetCountsByStatusThisMonthAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        return await _context.ServiceOrders
            .Where(o => o.CompanyId == _currentCompany.CompanyId && o.EntryDate >= startOfMonth)
            .AsNoTracking()
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken)
            .ContinueWith(t => t.Result.Select(x => (x.Status, x.Count)).ToList(), cancellationToken);
    }

    public async Task AddAsync(ServiceOrder order, CancellationToken cancellationToken)
    {
        await _context.ServiceOrders.AddAsync(order, cancellationToken);
    }

    public Task UpdateAsync(ServiceOrder order, CancellationToken cancellationToken)
    {
        _context.ServiceOrders.Update(order);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ServiceOrder order, CancellationToken cancellationToken)
    {
        _context.ServiceOrders.Remove(order);
        return Task.CompletedTask;
    }
}
