using FluentValidation;
using OrdemCerta.Application.Abstractions;
using OrdemCerta.Domain.Companies.Interfaces;
using OrdemCerta.Application.Inputs.ServiceOrderInputs;
using OrdemCerta.Domain.Companies.Enums;
using OrdemCerta.Domain.ServiceOrders;
using OrdemCerta.Domain.ServiceOrders.DTOs;
using OrdemCerta.Domain.ServiceOrders.Enums;
using OrdemCerta.Domain.ServiceOrders.Extensions;
using OrdemCerta.Domain.ServiceOrders.ValueObjects;
using OrdemCerta.Infrastructure.DataContext.Uow;
using OrdemCerta.Infrastructure.Repositories.CompanyRepository;
using OrdemCerta.Infrastructure.Repositories.ServiceOrderRepository;
using OrdemCerta.Shared;

namespace OrdemCerta.Application.Services.ServiceOrderService;

public class ServiceOrderService : IServiceOrderService
{
    private readonly IServiceOrderRepository _serviceOrderRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentCompany _currentCompany;
    private readonly IValidator<CreateServiceOrderInput> _createValidator;
    private readonly IValidator<UpdateServiceOrderInput> _updateValidator;
    private readonly IValidator<ChangeStatusInput> _changeStatusValidator;

    public ServiceOrderService(
        IServiceOrderRepository serviceOrderRepository,
        ICompanyRepository companyRepository,
        IUnitOfWork unitOfWork,
        ICurrentCompany currentCompany,
        IValidator<CreateServiceOrderInput> createValidator,
        IValidator<UpdateServiceOrderInput> updateValidator,
        IValidator<ChangeStatusInput> changeStatusValidator)
    {
        _serviceOrderRepository = serviceOrderRepository;
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
        _currentCompany = currentCompany;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _changeStatusValidator = changeStatusValidator;
    }

    public async Task<Result<ServiceOrderOutput>> CreateAsync(CreateServiceOrderInput input, CancellationToken cancellationToken)
    {
        var validationResult = await _createValidator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
            return Result<ServiceOrderOutput>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var companyResult = await _companyRepository.GetByIdAsync(_currentCompany.CompanyId, cancellationToken);
        if (companyResult.IsFailure)
            return Result<ServiceOrderOutput>.Failure(companyResult.Errors);

        if (companyResult.Value!.Plan == PlanType.Demo)
        {
            var count = await _serviceOrderRepository.CountAsync(cancellationToken);
            if (count >= 10)
                return "Limite de 10 ordens de serviço atingido. Faça upgrade para o plano pago.";
        }

        var equipmentResult = EquipmentInfo.Create(
            input.DeviceType,
            input.Brand,
            input.Model,
            input.ReportedDefect,
            input.Accessories,
            input.Observations);

        if (equipmentResult.IsFailure)
            return Result<ServiceOrderOutput>.Failure(equipmentResult.Errors);

        var orderResult = ServiceOrder.Create(
            _currentCompany.CompanyId,
            input.CustomerId,
            equipmentResult.Value!,
            input.TechnicianName);

        if (orderResult.IsFailure)
            return Result<ServiceOrderOutput>.Failure(orderResult.Errors);

        await _serviceOrderRepository.AddAsync(orderResult.Value!, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return orderResult.Value!.ToOutput();
    }

    public async Task<Result<ServiceOrderOutput>> UpdateAsync(Guid id, UpdateServiceOrderInput input, CancellationToken cancellationToken)
    {
        var validationResult = await _updateValidator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
            return Result<ServiceOrderOutput>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var orderResult = await _serviceOrderRepository.GetByIdAsync(id, cancellationToken);
        if (orderResult.IsFailure)
            return Result<ServiceOrderOutput>.Failure(orderResult.Errors);

        var order = orderResult.Value!;

        var equipmentResult = EquipmentInfo.Create(
            input.DeviceType,
            input.Brand,
            input.Model,
            input.ReportedDefect,
            input.Accessories,
            input.Observations);

        if (equipmentResult.IsFailure)
            return Result<ServiceOrderOutput>.Failure(equipmentResult.Errors);

        order.UpdateEquipment(equipmentResult.Value!);
        order.UpdateTechnician(input.TechnicianName);

        await _serviceOrderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return order.ToOutput();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var orderResult = await _serviceOrderRepository.GetByIdAsync(id, cancellationToken);
        if (orderResult.IsFailure)
            return Result.Failure(orderResult.Errors);

        await _serviceOrderRepository.DeleteAsync(orderResult.Value!, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<ServiceOrderOutput>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var orderResult = await _serviceOrderRepository.GetByIdAsync(id, cancellationToken);
        if (orderResult.IsFailure)
            return Result<ServiceOrderOutput>.Failure(orderResult.Errors);

        return orderResult.Value!.ToOutput();
    }

    public async Task<Result<List<ServiceOrderOutput>>> GetPagedAsync(GetPagedInput input, CancellationToken cancellationToken)
    {
        var orders = await _serviceOrderRepository.GetPagedAsync(input.Page, input.PageSize, cancellationToken);
        return orders.Select(o => o.ToOutput()).ToList();
    }

    public async Task<Result<List<ServiceOrderOutput>>> GetByStatusAsync(ServiceOrderStatus status, GetPagedInput input, CancellationToken cancellationToken)
    {
        var orders = await _serviceOrderRepository.GetByStatusAsync(status, input.Page, input.PageSize, cancellationToken);
        return orders.Select(o => o.ToOutput()).ToList();
    }

    public async Task<Result<List<ServiceOrderOutput>>> GetByCustomerAsync(Guid customerId, GetPagedInput input, CancellationToken cancellationToken)
    {
        var orders = await _serviceOrderRepository.GetByCustomerAsync(customerId, input.Page, input.PageSize, cancellationToken);
        return orders.Select(o => o.ToOutput()).ToList();
    }

    public async Task<Result<ServiceOrderOutput>> ChangeStatusAsync(Guid id, ChangeStatusInput input, CancellationToken cancellationToken)
    {
        var validationResult = await _changeStatusValidator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
            return Result<ServiceOrderOutput>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var orderResult = await _serviceOrderRepository.GetByIdAsync(id, cancellationToken);
        if (orderResult.IsFailure)
            return Result<ServiceOrderOutput>.Failure(orderResult.Errors);

        var order = orderResult.Value!;
        order.ChangeStatus(input.Status);

        await _serviceOrderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return order.ToOutput();
    }
}
