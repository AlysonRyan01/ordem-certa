using OrdemCerta.Domain.ServiceOrders.DTOs;
using OrdemCerta.Shared;

namespace OrdemCerta.Application.Services.DashboardService;

public interface IDashboardService
{
    Task<Result<DashboardOutput>> GetAsync(CancellationToken cancellationToken);
}
