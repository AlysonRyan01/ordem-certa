using OrdemCerta.Domain.Companies.Enums;
using OrdemCerta.Domain.Companies.Interfaces;
using OrdemCerta.Domain.ServiceOrders.DTOs;
using OrdemCerta.Domain.ServiceOrders.Enums;
using OrdemCerta.Domain.ServiceOrders.Extensions;
using OrdemCerta.Infrastructure.Repositories.CompanyRepository;
using OrdemCerta.Infrastructure.Repositories.ServiceOrderRepository;
using OrdemCerta.Shared;

namespace OrdemCerta.Application.Services.DashboardService;

public class DashboardService : IDashboardService
{
    private static readonly ServiceOrderStatus[] OpenStatuses =
    [
        ServiceOrderStatus.UnderAnalysis,
        ServiceOrderStatus.AwaitingApproval,
        ServiceOrderStatus.UnderRepair,
        ServiceOrderStatus.ReadyForPickup,
    ];

    private readonly IServiceOrderRepository _orderRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICurrentCompany _currentCompany;

    public DashboardService(
        IServiceOrderRepository orderRepository,
        ICompanyRepository companyRepository,
        ICurrentCompany currentCompany)
    {
        _orderRepository = orderRepository;
        _companyRepository = companyRepository;
        _currentCompany = currentCompany;
    }

    public async Task<Result<DashboardOutput>> GetAsync(CancellationToken cancellationToken)
    {
        var companyResult = await _companyRepository.GetByIdAsync(_currentCompany.CompanyId, cancellationToken);
        if (companyResult.IsFailure)
            return Result<DashboardOutput>.Failure(companyResult.Errors);

        var company = companyResult.Value!;

        var openOrders = await _orderRepository.CountByStatusesAsync(OpenStatuses, cancellationToken);
        var readyForPickup = await _orderRepository.CountByStatusesAsync([ServiceOrderStatus.ReadyForPickup], cancellationToken);
        var waitingApproval = await _orderRepository.CountByBudgetStatusAsync(ServiceOrderRepairStatus.Waiting, cancellationToken);
        var totalOrders = await _orderRepository.CountAsync(cancellationToken);
        var recentOrders = await _orderRepository.GetRecentAsync(5, cancellationToken);
        var countsByStatus = await _orderRepository.GetCountsByStatusThisMonthAsync(cancellationToken);

        var ordersByStatus = countsByStatus
            .Select(x => new StatusCountOutput(x.Status.ToString(), x.Count))
            .ToList();

        var plan = company.Plan == PlanType.Demo ? "Demo" : "Paid";

        return new DashboardOutput(
            openOrders,
            readyForPickup,
            waitingApproval,
            totalOrders,
            plan,
            recentOrders.Select(o => o.ToOutput()).ToList(),
            ordersByStatus);
    }
}
