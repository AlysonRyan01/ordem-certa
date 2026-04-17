using FluentValidation;
using Hangfire;
using Microsoft.Extensions.Configuration;
using OrdemCerta.Application.Abstractions;
using OrdemCerta.Application.Services.PdfService;
using OrdemCerta.Application.WhatsApp;
using OrdemCerta.Domain.Companies.Extensions;
using OrdemCerta.Domain.Companies.Interfaces;
using OrdemCerta.Domain.Customers.Extensions;
using QuestPDF.Infrastructure;
using OrdemCerta.Application.Inputs.ServiceOrderInputs;
using OrdemCerta.Domain.Companies.Enums;
using OrdemCerta.Domain.ServiceOrders;
using OrdemCerta.Domain.ServiceOrders.DTOs;
using OrdemCerta.Domain.ServiceOrders.Enums;
using OrdemCerta.Domain.ServiceOrders.Extensions;
using OrdemCerta.Infrastructure.DataContext.Uow;
using OrdemCerta.Infrastructure.Repositories.CompanyRepository;
using OrdemCerta.Infrastructure.Repositories.CustomerRepository;
using OrdemCerta.Infrastructure.Repositories.ServiceOrderNotificationRepository;
using OrdemCerta.Infrastructure.Repositories.ServiceOrderRepository;
using OrdemCerta.Shared;

namespace OrdemCerta.Application.Services.ServiceOrderService;

public class ServiceOrderService : IServiceOrderService
{
    private readonly IServiceOrderRepository _serviceOrderRepository;
    private readonly ICompanyOrderSequenceRepository _sequenceRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IServiceOrderNotificationRepository _notificationRepository;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IPdfService _pdfService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentCompany _currentCompany;
    private readonly IValidator<CreateServiceOrderInput> _createValidator;
    private readonly IValidator<UpdateServiceOrderInput> _updateValidator;
    private readonly IValidator<ChangeStatusInput> _changeStatusValidator;
    private readonly IValidator<CreateBudgetInput> _createBudgetValidator;
    private readonly IValidator<SetWarrantyInput> _setWarrantyValidator;
    private readonly string _baseUrl;

    public ServiceOrderService(
        IServiceOrderRepository serviceOrderRepository,
        ICompanyOrderSequenceRepository sequenceRepository,
        ICompanyRepository companyRepository,
        ICustomerRepository customerRepository,
        IServiceOrderNotificationRepository notificationRepository,
        IBackgroundJobClient backgroundJobClient,
        IPdfService pdfService,
        IUnitOfWork unitOfWork,
        ICurrentCompany currentCompany,
        IValidator<CreateServiceOrderInput> createValidator,
        IValidator<UpdateServiceOrderInput> updateValidator,
        IValidator<ChangeStatusInput> changeStatusValidator,
        IValidator<CreateBudgetInput> createBudgetValidator,
        IValidator<SetWarrantyInput> setWarrantyValidator,
        IConfiguration configuration)
    {
        _serviceOrderRepository = serviceOrderRepository;
        _sequenceRepository = sequenceRepository;
        _companyRepository = companyRepository;
        _customerRepository = customerRepository;
        _notificationRepository = notificationRepository;
        _backgroundJobClient = backgroundJobClient;
        _pdfService = pdfService;
        _unitOfWork = unitOfWork;
        _currentCompany = currentCompany;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _changeStatusValidator = changeStatusValidator;
        _createBudgetValidator = createBudgetValidator;
        _setWarrantyValidator = setWarrantyValidator;
        _baseUrl = configuration["App:BaseUrl"] ?? string.Empty;
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

        var orderNumber = await _sequenceRepository.GetNextNumberAsync(_currentCompany.CompanyId, cancellationToken);

        var orderResult = ServiceOrder.Create(
            _currentCompany.CompanyId,
            input.CustomerId,
            orderNumber,
            input.DeviceType,
            input.Brand,
            input.Model,
            input.ReportedDefect,
            input.Accessories,
            input.Observations,
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

        order.UpdateEquipment(
            input.DeviceType,
            input.Brand,
            input.Model,
            input.ReportedDefect,
            input.Accessories,
            input.Observations);

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

    public async Task<Result<ServiceOrderOutput>> GetPublicByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var orderResult = await _serviceOrderRepository.GetByIdPublicAsync(id, cancellationToken);
        if (orderResult.IsFailure)
            return Result<ServiceOrderOutput>.Failure(orderResult.Errors);

        return orderResult.Value!.ToOutput();
    }

    public async Task<Result<List<ServiceOrderOutput>>> GetPagedAsync(GetPagedInput input, CancellationToken cancellationToken)
    {
        var orders = await _serviceOrderRepository.GetPagedAsync(input.Page, input.PageSize, cancellationToken);
        var names = await _customerRepository.GetNamesByIdsAsync(orders.Select(o => o.CustomerId), cancellationToken);
        return orders.Select(o => o.ToOutput(customerName: names.GetValueOrDefault(o.CustomerId))).ToList();
    }

    public async Task<Result<List<ServiceOrderOutput>>> GetByStatusAsync(ServiceOrderStatus status, GetPagedInput input, CancellationToken cancellationToken)
    {
        var orders = await _serviceOrderRepository.GetByStatusAsync(status, input.Page, input.PageSize, cancellationToken);
        var names = await _customerRepository.GetNamesByIdsAsync(orders.Select(o => o.CustomerId), cancellationToken);
        return orders.Select(o => o.ToOutput(customerName: names.GetValueOrDefault(o.CustomerId))).ToList();
    }

    public async Task<Result<List<ServiceOrderOutput>>> GetByCustomerAsync(Guid customerId, GetPagedInput input, CancellationToken cancellationToken)
    {
        var orders = await _serviceOrderRepository.GetByCustomerAsync(customerId, input.Page, input.PageSize, cancellationToken);
        var names = await _customerRepository.GetNamesByIdsAsync(orders.Select(o => o.CustomerId), cancellationToken);
        return orders.Select(o => o.ToOutput(customerName: names.GetValueOrDefault(o.CustomerId))).ToList();
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

    public async Task<Result<ServiceOrderOutput>> RollbackAsync(Guid id, CancellationToken cancellationToken)
    {
        var orderResult = await _serviceOrderRepository.GetByIdAsync(id, cancellationToken);
        if (orderResult.IsFailure)
            return Result<ServiceOrderOutput>.Failure(orderResult.Errors);

        var order = orderResult.Value!;
        var rollbackResult = order.Rollback();
        if (rollbackResult.IsFailure)
            return Result<ServiceOrderOutput>.Failure(rollbackResult.Errors);

        await _serviceOrderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return order.ToOutput();
    }

    public async Task<Result<ServiceOrderOutput>> SetWarrantyAsync(Guid id, SetWarrantyInput input, CancellationToken cancellationToken)
    {
        var validationResult = await _setWarrantyValidator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
            return Result<ServiceOrderOutput>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var orderResult = await _serviceOrderRepository.GetByIdAsync(id, cancellationToken);
        if (orderResult.IsFailure)
            return Result<ServiceOrderOutput>.Failure(orderResult.Errors);

        var order = orderResult.Value!;
        order.SetWarranty(input.Duration, input.Unit);

        await _serviceOrderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return order.ToOutput();
    }

    public async Task<Result<ServiceOrderOutput>> SetRepairResultAsync(Guid id, SetRepairResultInput input, CancellationToken cancellationToken)
    {
        var orderResult = await _serviceOrderRepository.GetByIdAsync(id, cancellationToken);
        if (orderResult.IsFailure)
            return Result<ServiceOrderOutput>.Failure(orderResult.Errors);

        var order = orderResult.Value!;
        var setResult = order.SetRepairResult(input.RepairResult);
        if (setResult.IsFailure)
            return Result<ServiceOrderOutput>.Failure(setResult.Errors);

        await _serviceOrderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return order.ToOutput();
    }

    public async Task<Result<ServiceOrderOutput>> CreateBudgetAsync(Guid id, CreateBudgetInput input, CancellationToken cancellationToken)
    {
        var validationResult = await _createBudgetValidator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
            return Result<ServiceOrderOutput>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var orderResult = await _serviceOrderRepository.GetByIdAsync(id, cancellationToken);
        if (orderResult.IsFailure)
            return Result<ServiceOrderOutput>.Failure(orderResult.Errors);

        var order = orderResult.Value!;
        order.CreateBudget(input.Value, input.Description, input.RepairResult, input.WarrantyDuration, input.WarrantyUnit);

        await _serviceOrderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return order.ToOutput();
    }

    public async Task<Result<ServiceOrderOutput>> UpdateBudgetAsync(Guid id, CreateBudgetInput input, CancellationToken cancellationToken)
    {
        var validationResult = await _createBudgetValidator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
            return Result<ServiceOrderOutput>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var orderResult = await _serviceOrderRepository.GetByIdAsync(id, cancellationToken);
        if (orderResult.IsFailure)
            return Result<ServiceOrderOutput>.Failure(orderResult.Errors);

        var order = orderResult.Value!;
        var updateResult = order.UpdateBudget(input.Value, input.Description, input.RepairResult, input.WarrantyDuration, input.WarrantyUnit);
        if (updateResult.IsFailure)
            return Result<ServiceOrderOutput>.Failure(updateResult.Errors);

        await _serviceOrderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return order.ToOutput();
    }

    public async Task<Result<ServiceOrderOutput>> ApproveBudgetAsync(Guid id, CancellationToken cancellationToken)
    {
        var orderResult = await _serviceOrderRepository.GetByIdAsync(id, cancellationToken);
        if (orderResult.IsFailure)
            return Result<ServiceOrderOutput>.Failure(orderResult.Errors);

        var order = orderResult.Value!;
        var approveResult = order.ApproveBudget();
        if (approveResult.IsFailure)
            return Result<ServiceOrderOutput>.Failure(approveResult.Errors);

        await _serviceOrderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        var companyResult = await _companyRepository.GetByIdAsync(order.CompanyId, cancellationToken);
        var companyName = companyResult.IsSuccess ? companyResult.Value!.Name : null;

        return order.ToOutput(companyName);
    }

    public async Task<Result<ServiceOrderOutput>> RefuseBudgetAsync(Guid id, CancellationToken cancellationToken)
    {
        var orderResult = await _serviceOrderRepository.GetByIdAsync(id, cancellationToken);
        if (orderResult.IsFailure)
            return Result<ServiceOrderOutput>.Failure(orderResult.Errors);

        var order = orderResult.Value!;
        var refuseResult = order.RefuseBudget();
        if (refuseResult.IsFailure)
            return Result<ServiceOrderOutput>.Failure(refuseResult.Errors);

        await _serviceOrderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        var companyResult = await _companyRepository.GetByIdAsync(order.CompanyId, cancellationToken);
        var companyName = companyResult.IsSuccess ? companyResult.Value!.Name : null;

        return order.ToOutput(companyName);
    }

    public async Task<Result<ServiceOrderOutput>> ApproveBudgetFromLinkAsync(Guid id, CancellationToken cancellationToken)
    {
        var orderResult = await _serviceOrderRepository.GetByIdPublicAsync(id, cancellationToken);
        if (orderResult.IsFailure)
            return Result<ServiceOrderOutput>.Failure(orderResult.Errors);

        var order = orderResult.Value!;
        var approveResult = order.ApproveBudget();
        if (approveResult.IsFailure)
            return Result<ServiceOrderOutput>.Failure(approveResult.Errors);

        await _serviceOrderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        var companyResult = await _companyRepository.GetByIdAsync(order.CompanyId, cancellationToken);
        var customerResult = await _customerRepository.GetByIdAsync(order.CustomerId, cancellationToken);

        if (companyResult.IsSuccess && customerResult.IsSuccess && customerResult.Value!.Phone != null)
        {
            var company = companyResult.Value!;
            var customer = customerResult.Value!;
            var device = $"{order.DeviceType} {order.Brand} {order.Model}";

            _backgroundJobClient.Enqueue<WhatsAppJobs>(j => j.SendTextAsync(
                $"55{customer.Phone}",
                $"""
                *Orçamento aprovado!*

                Olá, {customer.FullName}! Recebemos sua aprovação do orçamento da ordem *#{order.OrderNumber}*.

                O reparo do seu *{device}* será iniciado em breve.

                Dúvidas? Fale conosco: {company.GetPhoneFormatted()}

                *{company.Name}*
                """,
                CancellationToken.None));

            _backgroundJobClient.Enqueue<WhatsAppJobs>(j => j.SendTextAsync(
                $"55{company.Phone}",
                $"""
                *Orçamento aprovado pelo cliente*

                O cliente aprovou o orçamento da ordem *#{order.OrderNumber}* pelo link.

                *Cliente:* {customer.FullName}
                *Aparelho:* {device}

                O reparo pode ser iniciado.
                """,
                CancellationToken.None));

            await RecordNotificationAsync(order.Id, order.CompanyId, NotificationType.BudgetApproved, NotificationRecipientType.Customer, customer.FullName, $"55{customer.Phone}", cancellationToken);
            await RecordNotificationAsync(order.Id, order.CompanyId, NotificationType.BudgetApproved, NotificationRecipientType.Company, company.Name, $"55{company.Phone}", cancellationToken);
        }

        var companyName = companyResult.IsSuccess ? companyResult.Value!.Name : null;
        return order.ToOutput(companyName);
    }

    public async Task<Result<ServiceOrderOutput>> RefuseBudgetFromLinkAsync(Guid id, CancellationToken cancellationToken)
    {
        var orderResult = await _serviceOrderRepository.GetByIdPublicAsync(id, cancellationToken);
        if (orderResult.IsFailure)
            return Result<ServiceOrderOutput>.Failure(orderResult.Errors);

        var order = orderResult.Value!;
        var refuseResult = order.RefuseBudget();
        if (refuseResult.IsFailure)
            return Result<ServiceOrderOutput>.Failure(refuseResult.Errors);

        await _serviceOrderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        var companyResult = await _companyRepository.GetByIdAsync(order.CompanyId, cancellationToken);
        var customerResult = await _customerRepository.GetByIdAsync(order.CustomerId, cancellationToken);

        if (companyResult.IsSuccess && customerResult.IsSuccess && customerResult.Value!.Phone != null)
        {
            var company = companyResult.Value!;
            var customer = customerResult.Value!;
            var device = $"{order.DeviceType} {order.Brand} {order.Model}";

            _backgroundJobClient.Enqueue<WhatsAppJobs>(j => j.SendTextAsync(
                $"55{customer.Phone}",
                $"""
                *Orçamento recusado*

                Olá, {customer.FullName}! Recebemos sua recusa do orçamento da ordem *#{order.OrderNumber}*.

                Seu *{device}* estará disponível para retirada em breve.

                Dúvidas? Fale conosco: {company.GetPhoneFormatted()}

                *{company.Name}*
                """,
                CancellationToken.None));

            _backgroundJobClient.Enqueue<WhatsAppJobs>(j => j.SendTextAsync(
                $"55{company.Phone}",
                $"""
                *Orçamento recusado pelo cliente*

                O cliente recusou o orçamento da ordem *#{order.OrderNumber}* pelo link.

                *Cliente:* {customer.FullName}
                *Aparelho:* {device}
                """,
                CancellationToken.None));

            await RecordNotificationAsync(order.Id, order.CompanyId, NotificationType.BudgetRefused, NotificationRecipientType.Customer, customer.FullName, $"55{customer.Phone}", cancellationToken);
            await RecordNotificationAsync(order.Id, order.CompanyId, NotificationType.BudgetRefused, NotificationRecipientType.Company, company.Name, $"55{company.Phone}", cancellationToken);
        }

        var companyName = companyResult.IsSuccess ? companyResult.Value!.Name : null;
        return order.ToOutput(companyName);
    }

    public async Task<Result> NotifyBudgetCreatedAsync(Guid id, CancellationToken cancellationToken)
    {
        var orderResult = await _serviceOrderRepository.GetByIdAsync(id, cancellationToken);
        if (orderResult.IsFailure)
            return Result.Failure(orderResult.Errors);

        var order = orderResult.Value!;
        if (order.BudgetValue is null)
            return "A ordem não possui orçamento.";

        var noFixResult = order.RepairResult is RepairResult.NoFix or RepairResult.NoDefectFound;
        if (!noFixResult)
        {
            var markResult = order.MarkBudgetAsWaiting();
            if (markResult.IsFailure)
                return markResult;

            await _serviceOrderRepository.UpdateAsync(order, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }

        var customerResult = await _customerRepository.GetByIdAsync(order.CustomerId, cancellationToken);
        if (customerResult.IsFailure || string.IsNullOrEmpty(customerResult.Value!.Phone))
            return Result.Success();

        var companyResult = await _companyRepository.GetByIdAsync(order.CompanyId, cancellationToken);
        if (companyResult.IsFailure)
            return Result.Success();

        var customer = customerResult.Value!;
        var company = companyResult.Value!;
        var phone = $"55{customer.Phone}";

        var canBeRepaired = order.RepairResult == RepairResult.CanBeRepaired;

        string message;
        if (canBeRepaired)
        {
            var budgetLink = $"{_baseUrl}/orcamento/order/{order.Id}";

            message = $"""
                *{company.Name}*

                Olá, {customer.FullName}! O orçamento da ordem *#{order.OrderNumber}* está pronto.

                *Aparelho:* {order.DeviceType} {order.Brand} {order.Model}
                *Valor:* R$ {order.BudgetValue:N2}
                *Descrição:* {order.BudgetDescription}

                Acesse o link abaixo para visualizar e responder ao orçamento

                Dúvidas? Fale conosco: {company.GetPhoneFormatted()}
                """;

            _backgroundJobClient.Enqueue<WhatsAppJobs>(j => j.SendTextAsync(phone, message, CancellationToken.None));
            _backgroundJobClient.Enqueue<WhatsAppJobs>(j => j.SendTextAsync(phone, budgetLink, CancellationToken.None));
            await RecordNotificationAsync(order.Id, order.CompanyId, NotificationType.BudgetCreated, NotificationRecipientType.Customer, customer.FullName, phone, cancellationToken);
        }
        else
        {
            var motivo = order.RepairResult == RepairResult.NoDefectFound
                ? "não apresentou defeito durante a análise"
                : "não tem conserto";

            var taxaLinha = order.BudgetValue > 0
                ? $"\n*Taxa de diagnóstico: R$ {order.BudgetValue:N2}*"
                : string.Empty;

            message = $"""
                *{company.Name}*

                Olá, {customer.FullName}! Finalizamos a análise do seu *{order.DeviceType} {order.Brand} {order.Model}* (ordem *#{order.OrderNumber}*).

                Infelizmente, o aparelho *{motivo}*.{taxaLinha}

                {order.BudgetDescription}

                Dúvidas? Fale conosco: {company.GetPhoneFormatted()}
                """;

            _backgroundJobClient.Enqueue<WhatsAppJobs>(j => j.SendTextAsync(phone, message, CancellationToken.None));
            await RecordNotificationAsync(order.Id, order.CompanyId, NotificationType.BudgetCreated, NotificationRecipientType.Customer, customer.FullName, phone, cancellationToken);
        }

        var companyPhone = $"55{company.Phone}";
        var companyNotification = $"""
            *Mensagem enviada ao cliente*

            O resultado da ordem *#{order.OrderNumber}* foi enviado para *{customer.FullName}* via WhatsApp.
            """;

        _backgroundJobClient.Enqueue<WhatsAppJobs>(j => j.SendTextAsync(companyPhone, companyNotification, CancellationToken.None));
        await RecordNotificationAsync(order.Id, order.CompanyId, NotificationType.BudgetCreated, NotificationRecipientType.Company, company.Name, companyPhone, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> NotifyBudgetApprovedAsync(Guid id, CancellationToken cancellationToken)
    {
        var orderResult = await _serviceOrderRepository.GetByIdAsync(id, cancellationToken);
        if (orderResult.IsFailure)
            return Result.Failure(orderResult.Errors);

        var order = orderResult.Value!;

        var customerResult = await _customerRepository.GetByIdAsync(order.CustomerId, cancellationToken);
        if (customerResult.IsFailure || string.IsNullOrEmpty(customerResult.Value!.Phone))
            return Result.Success();

        var companyResult = await _companyRepository.GetByIdAsync(order.CompanyId, cancellationToken);
        if (companyResult.IsFailure)
            return Result.Success();

        var customer = customerResult.Value!;
        var company = companyResult.Value!;
        var phone = $"55{customer.Phone}";

        var trackingLink = $"{_baseUrl}/orcamento/order/{order.Id}";

        var message = $"""
            *{company.Name}*

            Olá, {customer.FullName}! O orçamento da ordem *#{order.OrderNumber}* foi aprovado e o reparo do seu *{order.DeviceType} {order.Brand} {order.Model}* será iniciado em breve.

            Acompanhe o andamento da sua ordem pelo link abaixo:

            Dúvidas? Fale conosco: {company.GetPhoneFormatted()}
            """;

        _backgroundJobClient.Enqueue<WhatsAppJobs>(j => j.SendTextAsync(phone, message, CancellationToken.None));
        _backgroundJobClient.Enqueue<WhatsAppJobs>(j => j.SendTextAsync(phone, trackingLink, CancellationToken.None));
        await RecordNotificationAsync(order.Id, order.CompanyId, NotificationType.BudgetApproved, NotificationRecipientType.Customer, customer.FullName, phone, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> NotifyBudgetRefusedAsync(Guid id, CancellationToken cancellationToken)
    {
        var orderResult = await _serviceOrderRepository.GetByIdAsync(id, cancellationToken);
        if (orderResult.IsFailure)
            return Result.Failure(orderResult.Errors);

        var order = orderResult.Value!;

        var customerResult = await _customerRepository.GetByIdAsync(order.CustomerId, cancellationToken);
        if (customerResult.IsFailure || string.IsNullOrEmpty(customerResult.Value!.Phone))
            return Result.Success();

        var companyResult = await _companyRepository.GetByIdAsync(order.CompanyId, cancellationToken);
        if (companyResult.IsFailure)
            return Result.Success();

        var customer = customerResult.Value!;
        var company = companyResult.Value!;
        var phone = $"55{customer.Phone}";

        var message = $"""
            *{company.Name}*

            Olá, {customer.FullName}! O orçamento da ordem *#{order.OrderNumber}* foi recusado. Seu *{order.DeviceType} {order.Brand} {order.Model}* está disponível para retirada.

            Dúvidas? Fale conosco: {company.GetPhoneFormatted()}
            """;

        _backgroundJobClient.Enqueue<WhatsAppJobs>(j => j.SendTextAsync(phone, message, CancellationToken.None));
        await RecordNotificationAsync(order.Id, order.CompanyId, NotificationType.BudgetRefused, NotificationRecipientType.Customer, customer.FullName, phone, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> NotifyReadyForPickupAsync(Guid id, CancellationToken cancellationToken)
    {
        var orderResult = await _serviceOrderRepository.GetByIdAsync(id, cancellationToken);
        if (orderResult.IsFailure)
            return Result.Failure(orderResult.Errors);

        var order = orderResult.Value!;

        var customerResult = await _customerRepository.GetByIdAsync(order.CustomerId, cancellationToken);
        if (customerResult.IsFailure || string.IsNullOrEmpty(customerResult.Value!.Phone))
            return Result.Success();

        var companyResult = await _companyRepository.GetByIdAsync(order.CompanyId, cancellationToken);
        if (companyResult.IsFailure)
            return Result.Success();

        var customer = customerResult.Value!;
        var company = companyResult.Value!;
        var phone = $"55{customer.Phone}";

        var device = $"{order.DeviceType} {order.Brand} {order.Model}";

        var (headline, body) = order.RepairResult switch
        {
            RepairResult.NoFix =>
                ("*Atualização sobre seu equipamento*",
                 $"Infelizmente, após avaliação técnica, não foi possível consertar o *{device}* da ordem *#{order.OrderNumber}*.\n\n📍 O equipamento está disponível para retirada."),

            RepairResult.NoDefectFound =>
                ("*Atualização sobre seu equipamento*",
                 $"O *{device}* da ordem *#{order.OrderNumber}* foi avaliado e *não apresentou defeito* detectável.\n\n📍 O equipamento está disponível para retirada."),

            RepairResult.CanBeRepaired when order.BudgetStatus == ServiceOrderRepairStatus.Disapproved =>
                ("*Equipamento disponível para retirada*",
                 $"O orçamento da ordem *#{order.OrderNumber}* foi recusado. O *{device}* está disponível para retirada sem reparo."),

            _ =>
                ("*Equipamento pronto para retirada!* 🎉",
                 $"O *{device}* da ordem *#{order.OrderNumber}* foi consertado e está pronto para retirada."),
        };

        var message = $"""
            {headline}

            Olá, {customer.FullName}!

            {body}

            📞 Dúvidas? Entre em contato: {company.GetPhoneFormatted()}

            *{company.Name}*
            """;

        _backgroundJobClient.Enqueue<WhatsAppJobs>(j => j.SendTextAsync(phone, message, CancellationToken.None));

        var resultLabel = order.RepairResult switch
        {
            RepairResult.NoFix         => "sem conserto",
            RepairResult.NoDefectFound => "sem defeito detectado",
            RepairResult.CanBeRepaired when order.BudgetStatus == ServiceOrderRepairStatus.Disapproved => "orçamento recusado",
            _                          => "consertado",
        };

        var companyPhone = $"55{company.Phone}";
        var companyNotification = $"""
            *Mensagem enviada ao cliente*

            A notificação de *pronto para retirada* ({resultLabel}) foi enviada para *{customer.FullName}* via WhatsApp.

            *Aparelho:* {device}
            *Ordem:* #{order.OrderNumber}
            """;

        _backgroundJobClient.Enqueue<WhatsAppJobs>(j => j.SendTextAsync(companyPhone, companyNotification, CancellationToken.None));
        await RecordNotificationAsync(order.Id, order.CompanyId, NotificationType.ReadyForPickup, NotificationRecipientType.Customer, customer.FullName, phone, cancellationToken);
        await RecordNotificationAsync(order.Id, order.CompanyId, NotificationType.ReadyForPickup, NotificationRecipientType.Company, company.Name, companyPhone, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> NotifyUnderAnalysisAsync(Guid id, CancellationToken cancellationToken)
    {
        var orderResult = await _serviceOrderRepository.GetByIdAsync(id, cancellationToken);
        if (orderResult.IsFailure)
            return Result.Failure(orderResult.Errors);

        var order = orderResult.Value!;

        var customerResult = await _customerRepository.GetByIdAsync(order.CustomerId, cancellationToken);
        if (customerResult.IsFailure || string.IsNullOrEmpty(customerResult.Value!.Phone))
            return Result.Success();

        var companyResult = await _companyRepository.GetByIdAsync(order.CompanyId, cancellationToken);
        if (companyResult.IsFailure)
            return Result.Success();

        var customer = customerResult.Value!;
        var company = companyResult.Value!;
        var phone = $"55{customer.Phone}";

        var message = $"""
            *{company.Name}*

            Olá, {customer.FullName}! Recebemos o seu *{order.DeviceType} {order.Brand} {order.Model}* (ordem *#{order.OrderNumber}*) e ele já está sob análise técnica.

            Em breve entraremos em contato com o resultado.

            📞 Dúvidas? Entre em contato: {company.GetPhoneFormatted()}
            """;

        _backgroundJobClient.Enqueue<WhatsAppJobs>(j => j.SendTextAsync(phone, message, CancellationToken.None));
        await RecordNotificationAsync(order.Id, order.CompanyId, NotificationType.UnderAnalysis, NotificationRecipientType.Customer, customer.FullName, phone, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> NotifyUnderRepairAsync(Guid id, CancellationToken cancellationToken)
    {
        var orderResult = await _serviceOrderRepository.GetByIdAsync(id, cancellationToken);
        if (orderResult.IsFailure)
            return Result.Failure(orderResult.Errors);

        var order = orderResult.Value!;

        var customerResult = await _customerRepository.GetByIdAsync(order.CustomerId, cancellationToken);
        if (customerResult.IsFailure || string.IsNullOrEmpty(customerResult.Value!.Phone))
            return Result.Success();

        var companyResult = await _companyRepository.GetByIdAsync(order.CompanyId, cancellationToken);
        if (companyResult.IsFailure)
            return Result.Success();

        var customer = customerResult.Value!;
        var company = companyResult.Value!;
        var phone = $"55{customer.Phone}";

        var message = $"""
            *{company.Name}*

            Olá, {customer.FullName}! O conserto do seu *{order.DeviceType} {order.Brand} {order.Model}* (ordem *#{order.OrderNumber}*) foi iniciado. 🔧

            Assim que o reparo for concluído, entraremos em contato.

            📞 Dúvidas? Entre em contato: {company.GetPhoneFormatted()}
            """;

        _backgroundJobClient.Enqueue<WhatsAppJobs>(j => j.SendTextAsync(phone, message, CancellationToken.None));
        await RecordNotificationAsync(order.Id, order.CompanyId, NotificationType.UnderRepair, NotificationRecipientType.Customer, customer.FullName, phone, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> NotifyDeliveredAsync(Guid id, CancellationToken cancellationToken)
    {
        var orderResult = await _serviceOrderRepository.GetByIdAsync(id, cancellationToken);
        if (orderResult.IsFailure)
            return Result.Failure(orderResult.Errors);

        var order = orderResult.Value!;

        var customerResult = await _customerRepository.GetByIdAsync(order.CustomerId, cancellationToken);
        if (customerResult.IsFailure || string.IsNullOrEmpty(customerResult.Value!.Phone))
            return Result.Success();

        var companyResult = await _companyRepository.GetByIdAsync(order.CompanyId, cancellationToken);
        if (companyResult.IsFailure)
            return Result.Success();

        var customer = customerResult.Value!;
        var company = companyResult.Value!;
        var phone = $"55{customer.Phone}";

        var message = $"""
            *{company.Name}*

            Olá, {customer.FullName}! Confirmamos a entrega do seu *{order.DeviceType} {order.Brand} {order.Model}* (ordem *#{order.OrderNumber}*). ✅

            Obrigado pela confiança! Qualquer dúvida, estamos à disposição.

            📞 {company.GetPhoneFormatted()}
            """;

        _backgroundJobClient.Enqueue<WhatsAppJobs>(j => j.SendTextAsync(phone, message, CancellationToken.None));
        await RecordNotificationAsync(order.Id, order.CompanyId, NotificationType.Delivered, NotificationRecipientType.Customer, customer.FullName, phone, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> NotifyCancelledAsync(Guid id, CancellationToken cancellationToken)
    {
        var orderResult = await _serviceOrderRepository.GetByIdAsync(id, cancellationToken);
        if (orderResult.IsFailure)
            return Result.Failure(orderResult.Errors);

        var order = orderResult.Value!;

        var customerResult = await _customerRepository.GetByIdAsync(order.CustomerId, cancellationToken);
        if (customerResult.IsFailure || string.IsNullOrEmpty(customerResult.Value!.Phone))
            return Result.Success();

        var companyResult = await _companyRepository.GetByIdAsync(order.CompanyId, cancellationToken);
        if (companyResult.IsFailure)
            return Result.Success();

        var customer = customerResult.Value!;
        var company = companyResult.Value!;
        var phone = $"55{customer.Phone}";

        var message = $"""
            *{company.Name}*

            Olá, {customer.FullName}! A ordem de serviço *#{order.OrderNumber}* referente ao seu *{order.DeviceType} {order.Brand} {order.Model}* foi cancelada.

            Seu equipamento está disponível para retirada em nossa loja.

            📞 Dúvidas? Entre em contato: {company.GetPhoneFormatted()}
            """;

        _backgroundJobClient.Enqueue<WhatsAppJobs>(j => j.SendTextAsync(phone, message, CancellationToken.None));
        await RecordNotificationAsync(order.Id, order.CompanyId, NotificationType.Cancelled, NotificationRecipientType.Customer, customer.FullName, phone, cancellationToken);
        return Result.Success();
    }

    public async Task<Result<byte[]>> PrintAsync(Guid id, CancellationToken cancellationToken)
    {
        var orderResult = await _serviceOrderRepository.GetByIdAsync(id, cancellationToken);
        if (orderResult.IsFailure)
            return Result<byte[]>.Failure(orderResult.Errors);

        var order = orderResult.Value!;

        var customerResult = await _customerRepository.GetByIdAsync(order.CustomerId, cancellationToken);
        if (customerResult.IsFailure)
            return Result<byte[]>.Failure(customerResult.Errors);

        var companyResult = await _companyRepository.GetByIdAsync(order.CompanyId, cancellationToken);
        if (companyResult.IsFailure)
            return Result<byte[]>.Failure(companyResult.Errors);

        var orderOutput = order.ToOutput();
        var customerOutput = customerResult.Value!.ToOutput();
        var companyOutput = companyResult.Value!.ToOutput();

        QuestPDF.Settings.License = LicenseType.Community;

        var pdf = order.Status == ServiceOrderStatus.Delivered
            ? order.RepairResult is RepairResult.NoFix or RepairResult.NoDefectFound
                ? _pdfService.GenerateReturnReceipt(orderOutput, customerOutput, companyOutput)
                : order.BudgetStatus == ServiceOrderRepairStatus.Approved
                    ? _pdfService.GenerateWarrantyCard(orderOutput, customerOutput, companyOutput)
                    : _pdfService.GenerateReturnReceipt(orderOutput, customerOutput, companyOutput)
            : _pdfService.GenerateEntryReceipt(orderOutput, customerOutput, companyOutput);

        return pdf;
    }

    public async Task<Result<List<ServiceOrderNotificationOutput>>> GetNotificationsAsync(Guid id, CancellationToken cancellationToken)
    {
        var orderResult = await _serviceOrderRepository.GetByIdAsync(id, cancellationToken);
        if (orderResult.IsFailure)
            return Result<List<ServiceOrderNotificationOutput>>.Failure(orderResult.Errors);

        var notifications = await _notificationRepository.GetByOrderIdAsync(id, cancellationToken);
        return notifications
            .Select(n => new ServiceOrderNotificationOutput(n.Id, n.Type.ToString(), n.RecipientType.ToString(), n.RecipientName, n.Phone, n.SentAt))
            .ToList();
    }

    private async Task RecordNotificationAsync(
        Guid serviceOrderId,
        Guid companyId,
        NotificationType type,
        NotificationRecipientType recipientType,
        string recipientName,
        string phone,
        CancellationToken cancellationToken)
    {
        var notification = ServiceOrderNotification.Create(serviceOrderId, companyId, type, recipientType, recipientName, phone);
        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
    }
}
