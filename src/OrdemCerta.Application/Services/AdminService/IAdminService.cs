using OrdemCerta.Domain.Admin.DTOs;
using OrdemCerta.Shared;

namespace OrdemCerta.Application.Services.AdminService;

public interface IAdminService
{
    Task<Result<AdminTokenOutput>> LoginAsync(string email, string password, CancellationToken cancellationToken);
    Task<Result<AdminStatsOutput>> GetStatsAsync(CancellationToken cancellationToken);
    Task<Result<List<AdminCompanyOutput>>> GetCompaniesAsync(int page, int pageSize, CancellationToken cancellationToken);
}
