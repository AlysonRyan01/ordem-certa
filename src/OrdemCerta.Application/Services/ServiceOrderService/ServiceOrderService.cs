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
using OrdemCerta.Domain.ServiceOrders.ValueObjects;
using OrdemCerta.Infrastructure.DataContext.Uow;
using OrdemCerta.Infrastructure.Repositories.CompanyRepository;
using OrdemCerta.Infrastructure.Repositories.CustomerRepository;
using OrdemCerta.Infrastructure.Repositories.ServiceOrderRepository;
using OrdemCerta.Shared;

namespace OrdemCerta.Application.Services.ServiceOrderService;

public class ServiceOrderService : IServiceOrderService
{
    private readonly IServiceOrderRepository _serviceOrderRepository;
    private readonly ICompanyOrderSequenceRepository _sequenceRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICustomerRepository _customerRepository;
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

        var equipmentResult = EquipmentInfo.Create(
            input.DeviceType,
            input.Brand,
            input.Model,
            input.ReportedDefect,
            input.Accessories,
            input.Observations);

        if (equipmentResult.IsFailure)
            return Result<ServiceOrderOutput>.Failure(equipmentResult.Errors);

        var orderNumber = await _sequenceRepository.GetNextNumberAsync(_currentCompany.CompanyId, cancellationToken);

        var orderResult = ServiceOrder.Create(
            _currentCompany.CompanyId,
            input.CustomerId,
            orderNumber,
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

    public async Task<Result<ServiceOrderOutput>> SetWarrantyAsync(Guid id, SetWarrantyInput input, CancellationToken cancellationToken)
    {
        var validationResult = await _setWarrantyValidator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
            return Result<ServiceOrderOutput>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var orderResult = await _serviceOrderRepository.GetByIdAsync(id, cancellationToken);
        if (orderResult.IsFailure)
            return Result<ServiceOrderOutput>.Failure(orderResult.Errors);

        var order = orderResult.Value!;

        var warrantyResult = Warranty.Create(input.Duration, input.Unit);
        if (warrantyResult.IsFailure)
            return Result<ServiceOrderOutput>.Failure(warrantyResult.Errors);

        order.SetWarranty(warrantyResult.Value!);

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

        var budgetResult = Budget.Create(input.Value, input.Description);
        if (budgetResult.IsFailure)
            return Result<ServiceOrderOutput>.Failure(budgetResult.Errors);

        Warranty? warranty = null;
        if (input.WarrantyDuration.HasValue && input.WarrantyUnit.HasValue)
        {
            var warrantyResult = Warranty.Create(input.WarrantyDuration.Value, input.WarrantyUnit.Value);
            if (warrantyResult.IsFailure)
                return Result<ServiceOrderOutput>.Failure(warrantyResult.Errors);
            warranty = warrantyResult.Value;
        }

        order.CreateBudget(budgetResult.Value!, input.RepairResult, warranty);

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

        var budgetResult = Budget.Create(input.Value, input.Description);
        if (budgetResult.IsFailure)
            return Result<ServiceOrderOutput>.Failure(budgetResult.Errors);

        Warranty? warranty = null;
        if (input.WarrantyDuration.HasValue && input.WarrantyUnit.HasValue)
        {
            var warrantyResult = Warranty.Create(input.WarrantyDuration.Value, input.WarrantyUnit.Value);
            if (warrantyResult.IsFailure)
                return Result<ServiceOrderOutput>.Failure(warrantyResult.Errors);
            warranty = warrantyResult.Value;
        }

        var updateResult = order.UpdateBudget(budgetResult.Value!, input.RepairResult, warranty);
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
        var companyName = companyResult.IsSuccess ? companyResult.Value!.Name.Value : null;

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
        var companyName = companyResult.IsSuccess ? companyResult.Value!.Name.Value : null;

        return order.ToOutput(companyName);
    }

    public async Task<Result<ServiceOrderOutput>> ApproveBudgetFromLinkAsync(Guid id, CancellationToken cancellationToken)
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
        var customerResult = await _customerRepository.GetByIdAsync(order.CustomerId, cancellationToken);

        if (companyResult.IsSuccess && customerResult.IsSuccess && customerResult.Value!.Phones.Any())
        {
            var company = companyResult.Value!;
            var customer = customerResult.Value!;
            var device = $"{order.Equipment.DeviceType} {order.Equipment.Brand} {order.Equipment.Model}";

            _backgroundJobClient.Enqueue<WhatsAppJobs>(j => j.SendTextAsync(
                $"55{customer.Phones.First().Value}",
                $"""
                ✅ *Orçamento aprovado!*

                Olá, {customer.Name.FullName}! Recebemos sua aprovação do orçamento da ordem *#{order.OrderNumber}*.

                O reparo do seu *{device}* será iniciado em breve.

                Dúvidas? Fale conosco: {company.Phone.GetFormatted()}

                *{company.Name.Value}*
                """,
                CancellationToken.None));

            _backgroundJobClient.Enqueue<WhatsAppJobs>(j => j.SendTextAsync(
                $"55{company.Phone.Value}",
                $"""
                ✅ *Orçamento aprovado pelo cliente*

                O cliente aprovou o orçamento da ordem *#{order.OrderNumber}* pelo link.

                👤 *Cliente:* {customer.Name.FullName}
                📱 *Aparelho:* {device}

                O reparo pode ser iniciado.
                """,
                CancellationToken.None));
        }

        var companyName = companyResult.IsSuccess ? companyResult.Value!.Name.Value : null;
        return order.ToOutput(companyName);
    }

    public async Task<Result<ServiceOrderOutput>> RefuseBudgetFromLinkAsync(Guid id, CancellationToken cancellationToken)
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
        var customerResult = await _customerRepository.GetByIdAsync(order.CustomerId, cancellationToken);

        if (companyResult.IsSuccess && customerResult.IsSuccess && customerResult.Value!.Phones.Any())
        {
            var company = companyResult.Value!;
            var customer = customerResult.Value!;
            var device = $"{order.Equipment.DeviceType} {order.Equipment.Brand} {order.Equipment.Model}";

            _backgroundJobClient.Enqueue<WhatsAppJobs>(j => j.SendTextAsync(
                $"55{customer.Phones.First().Value}",
                $"""
                ❌ *Orçamento recusado*

                Olá, {customer.Name.FullName}! Recebemos sua recusa do orçamento da ordem *#{order.OrderNumber}*.

                Seu *{device}* estará disponível para retirada em breve.

                Dúvidas? Fale conosco: {company.Phone.GetFormatted()}

                *{company.Name.Value}*
                """,
                CancellationToken.None));

            _backgroundJobClient.Enqueue<WhatsAppJobs>(j => j.SendTextAsync(
                $"55{company.Phone.Value}",
                $"""
                ❌ *Orçamento recusado pelo cliente*

                O cliente recusou o orçamento da ordem *#{order.OrderNumber}* pelo link.

                👤 *Cliente:* {customer.Name.FullName}
                📱 *Aparelho:* {device}
                """,
                CancellationToken.None));
        }

        var companyName = companyResult.IsSuccess ? companyResult.Value!.Name.Value : null;
        return order.ToOutput(companyName);
    }

    public async Task<Result> NotifyBudgetCreatedAsync(Guid id, CancellationToken cancellationToken)
    {
        var orderResult = await _serviceOrderRepository.GetByIdAsync(id, cancellationToken);
        if (orderResult.IsFailure)
            return Result.Failure(orderResult.Errors);

        var order = orderResult.Value!;
        if (order.Budget is null)
            return "A ordem não possui orçamento.";

        var customerResult = await _customerRepository.GetByIdAsync(order.CustomerId, cancellationToken);
        if (customerResult.IsFailure || !customerResult.Value!.Phones.Any())
            return Result.Success();

        var companyResult = await _companyRepository.GetByIdAsync(order.CompanyId, cancellationToken);
        if (companyResult.IsFailure)
            return Result.Success();

        var customer = customerResult.Value!;
        var company = companyResult.Value!;
        var phone = $"55{customer.Phones.First().Value}";

        var budgetLink = $"{_baseUrl}/orcamento/order/{order.Id}";

        var message = $"""
            🔧 *{company.Name.Value}*

            Olá, {customer.Name.FullName}! O orçamento da ordem *#{order.OrderNumber}* está pronto.

            📱 *Aparelho:* {order.Equipment.DeviceType} {order.Equipment.Brand} {order.Equipment.Model}
            💰 *Valor:* R$ {order.Budget.Value:N2}
            📋 *Descrição:* {order.Budget.Description}

            Acesse o link abaixo para visualizar e responder ao orçamento:
            👉 {budgetLink}

            Dúvidas? Fale conosco: {company.Phone.GetFormatted()}
            """;

        _backgroundJobClient.Enqueue<WhatsAppJobs>(j => j.SendTextAsync(phone, message, CancellationToken.None));

        var companyPhone = $"55{company.Phone.Value}";
        var companyNotification = $"""
            📤 *Mensagem enviada ao cliente*

            O orçamento da ordem *#{order.OrderNumber}* foi enviado para *{customer.Name.FullName}* via WhatsApp.

            💰 *Valor:* R$ {order.Budget.Value:N2}
            📋 *Descrição:* {order.Budget.Description}
            📱 *Aparelho:* {order.Equipment.DeviceType} {order.Equipment.Brand} {order.Equipment.Model}
            """;

        _backgroundJobClient.Enqueue<WhatsAppJobs>(j => j.SendTextAsync(companyPhone, companyNotification, CancellationToken.None));
        return Result.Success();
    }

    public async Task<Result> NotifyBudgetApprovedAsync(Guid id, CancellationToken cancellationToken)
    {
        var orderResult = await _serviceOrderRepository.GetByIdAsync(id, cancellationToken);
        if (orderResult.IsFailure)
            return Result.Failure(orderResult.Errors);

        var order = orderResult.Value!;

        var companyResult = await _companyRepository.GetByIdAsync(order.CompanyId, cancellationToken);
        if (companyResult.IsFailure)
            return Result.Success();

        var company = companyResult.Value!;
        var phone = $"55{company.Phone.Value}";

        var message = $"""
            ✅ APROVADO

            O orçamento da ordem *#{order.OrderNumber}* foi aprovado.

            Acesse o sistema para mais detalhes.
            """;

        _backgroundJobClient.Enqueue<WhatsAppJobs>(j => j.SendTextAsync(phone, message, CancellationToken.None));
        return Result.Success();
    }

    public async Task<Result> NotifyBudgetRefusedAsync(Guid id, CancellationToken cancellationToken)
    {
        var orderResult = await _serviceOrderRepository.GetByIdAsync(id, cancellationToken);
        if (orderResult.IsFailure)
            return Result.Failure(orderResult.Errors);

        var order = orderResult.Value!;

        var companyResult = await _companyRepository.GetByIdAsync(order.CompanyId, cancellationToken);
        if (companyResult.IsFailure)
            return Result.Success();

        var company = companyResult.Value!;
        var phone = $"55{company.Phone.Value}";

        var message = $"""
            ❌ RECUSADO

            O orçamento da ordem *#{order.OrderNumber}* foi recusado.

            Acesse o sistema para mais detalhes.
            """;

        _backgroundJobClient.Enqueue<WhatsAppJobs>(j => j.SendTextAsync(phone, message, CancellationToken.None));
        return Result.Success();
    }

    public async Task<Result> NotifyReadyForPickupAsync(Guid id, CancellationToken cancellationToken)
    {
        var orderResult = await _serviceOrderRepository.GetByIdAsync(id, cancellationToken);
        if (orderResult.IsFailure)
            return Result.Failure(orderResult.Errors);

        var order = orderResult.Value!;

        var customerResult = await _customerRepository.GetByIdAsync(order.CustomerId, cancellationToken);
        if (customerResult.IsFailure || !customerResult.Value!.Phones.Any())
            return Result.Success();

        var companyResult = await _companyRepository.GetByIdAsync(order.CompanyId, cancellationToken);
        if (companyResult.IsFailure)
            return Result.Success();

        var customer = customerResult.Value!;
        var company = companyResult.Value!;
        var phone = $"55{customer.Phones.First().Value}";

        var device = $"{order.Equipment.DeviceType} {order.Equipment.Brand} {order.Equipment.Model}";

        var (headline, body) = order.RepairResult switch
        {
            RepairResult.NoFix =>
                ("⚠️ *Atualização sobre seu equipamento*",
                 $"Infelizmente, após avaliação técnica, não foi possível consertar o *{device}* da ordem *#{order.OrderNumber}*.\n\n📍 O equipamento está disponível para retirada."),

            RepairResult.NoDefectFound =>
                ("ℹ️ *Atualização sobre seu equipamento*",
                 $"O *{device}* da ordem *#{order.OrderNumber}* foi avaliado e *não apresentou defeito* detectável.\n\n📍 O equipamento está disponível para retirada."),

            _ =>
                ("✅ *Equipamento pronto para retirada!*",
                 $"O *{device}* da ordem *#{order.OrderNumber}* foi consertado e está pronto para retirada."),
        };

        var message = $"""
            {headline}

            Olá, {customer.Name.FullName}!

            {body}

            📞 Dúvidas? Entre em contato: {company.Phone.GetFormatted()}

            *{company.Name.Value}*
            """;

        _backgroundJobClient.Enqueue<WhatsAppJobs>(j => j.SendTextAsync(phone, message, CancellationToken.None));

        var resultLabel = order.RepairResult switch
        {
            RepairResult.NoFix         => "sem conserto",
            RepairResult.NoDefectFound => "sem defeito detectado",
            _                          => "consertado",
        };

        var companyPhone = $"55{company.Phone.Value}";
        var companyNotification = $"""
            📤 *Mensagem enviada ao cliente*

            A notificação de *pronto para retirada* ({resultLabel}) foi enviada para *{customer.Name.FullName}* via WhatsApp.

            📱 *Aparelho:* {device}
            🔢 *Ordem:* #{order.OrderNumber}
            """;

        _backgroundJobClient.Enqueue<WhatsAppJobs>(j => j.SendTextAsync(companyPhone, companyNotification, CancellationToken.None));
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
                : _pdfService.GenerateWarrantyCard(orderOutput, customerOutput, companyOutput)
            : _pdfService.GenerateEntryReceipt(orderOutput, customerOutput, companyOutput);

        return pdf;
    }
}
