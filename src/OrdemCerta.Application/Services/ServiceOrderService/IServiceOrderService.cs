using OrdemCerta.Application.Inputs.ServiceOrderInputs;
using OrdemCerta.Domain.ServiceOrders.DTOs;
using OrdemCerta.Domain.ServiceOrders.Enums;
using OrdemCerta.Shared;

namespace OrdemCerta.Application.Services.ServiceOrderService;

public interface IServiceOrderService
{
    Task<Result<ServiceOrderOutput>> CreateAsync(CreateServiceOrderInput input, CancellationToken cancellationToken);
    Task<Result<ServiceOrderOutput>> UpdateAsync(Guid id, UpdateServiceOrderInput input, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<ServiceOrderOutput>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<ServiceOrderOutput>> GetPublicByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<List<ServiceOrderOutput>>> GetPagedAsync(GetPagedInput input, CancellationToken cancellationToken);
    Task<Result<List<ServiceOrderOutput>>> GetByStatusAsync(ServiceOrderStatus status, GetPagedInput input, CancellationToken cancellationToken);
    Task<Result<List<ServiceOrderOutput>>> GetByCustomerAsync(Guid customerId, GetPagedInput input, CancellationToken cancellationToken);
    Task<Result<ServiceOrderOutput>> ChangeStatusAsync(Guid id, ChangeStatusInput input, CancellationToken cancellationToken);
    Task<Result<ServiceOrderOutput>> SetRepairResultAsync(Guid id, SetRepairResultInput input, CancellationToken cancellationToken);
    Task<Result<ServiceOrderOutput>> SetWarrantyAsync(Guid id, SetWarrantyInput input, CancellationToken cancellationToken);
    Task<Result<ServiceOrderOutput>> CreateBudgetAsync(Guid id, CreateBudgetInput input, CancellationToken cancellationToken);
    Task<Result<ServiceOrderOutput>> UpdateBudgetAsync(Guid id, CreateBudgetInput input, CancellationToken cancellationToken);
    Task<Result<ServiceOrderOutput>> ApproveBudgetAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<ServiceOrderOutput>> RefuseBudgetAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<ServiceOrderOutput>> ApproveBudgetFromLinkAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<ServiceOrderOutput>> RefuseBudgetFromLinkAsync(Guid id, CancellationToken cancellationToken);
    Task<Result> NotifyBudgetCreatedAsync(Guid id, CancellationToken cancellationToken);
    Task<Result> NotifyBudgetApprovedAsync(Guid id, CancellationToken cancellationToken);
    Task<Result> NotifyBudgetRefusedAsync(Guid id, CancellationToken cancellationToken);
    Task<Result> NotifyReadyForPickupAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<byte[]>> PrintAsync(Guid id, CancellationToken cancellationToken);
}
