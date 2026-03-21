using Microsoft.EntityFrameworkCore;
using OrdemCerta.Domain.Admin;
using OrdemCerta.Domain.Admin.DTOs;
using OrdemCerta.Domain.Companies.Enums;
using OrdemCerta.Infrastructure.DataContext.Context;

namespace OrdemCerta.Infrastructure.Repositories.AdminRepository;

public class AdminRepository : IAdminRepository
{
    private readonly ApplicationDataContext _context;

    public AdminRepository(ApplicationDataContext context)
    {
        _context = context;
    }

    public async Task<AdminUser?> GetByEmailAsync(string email, CancellationToken cancellationToken)
        => await _context.AdminUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Email == email, cancellationToken);

    public async Task<bool> AnyAsync(CancellationToken cancellationToken)
        => await _context.AdminUsers.AnyAsync(cancellationToken);

    public async Task AddAsync(AdminUser admin, CancellationToken cancellationToken)
        => await _context.AdminUsers.AddAsync(admin, cancellationToken);

    public async Task<AdminStatsOutput> GetStatsAsync(CancellationToken cancellationToken)
    {
        var totalCompanies = await _context.Companies.CountAsync(cancellationToken);
        var paidCompanies = await _context.Companies.CountAsync(c => c.Plan == PlanType.Paid, cancellationToken);
        var demoCompanies = totalCompanies - paidCompanies;
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var newLast30Days = await _context.Companies.CountAsync(c => c.CreatedAt >= thirtyDaysAgo, cancellationToken);
        var totalServiceOrders = await _context.ServiceOrders.CountAsync(cancellationToken);

        return new AdminStatsOutput(totalCompanies, paidCompanies, demoCompanies, newLast30Days, totalServiceOrders);
    }

    public async Task<List<AdminCompanyOutput>> GetCompaniesPagedAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        if (page < 1) page = 1;
        pageSize = Math.Clamp(pageSize, 1, 100);

        return await _context.Companies
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new AdminCompanyOutput(
                c.Id,
                c.Name,
                c.Email,
                c.Plan.ToString(),
                c.StripeSubscriptionId != null,
                _context.ServiceOrders.Count(o => o.CompanyId == c.Id),
                c.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
