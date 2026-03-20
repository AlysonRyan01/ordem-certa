using OrdemCerta.Domain.Admin;
using OrdemCerta.Domain.Admin.DTOs;

namespace OrdemCerta.Infrastructure.Repositories.AdminRepository;

public interface IAdminRepository
{
    Task<AdminUser?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<bool> AnyAsync(CancellationToken cancellationToken);
    Task AddAsync(AdminUser admin, CancellationToken cancellationToken);
    Task<AdminStatsOutput> GetStatsAsync(CancellationToken cancellationToken);
    Task<List<AdminCompanyOutput>> GetCompaniesPagedAsync(int page, int pageSize, CancellationToken cancellationToken);
}
