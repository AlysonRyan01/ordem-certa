using MediatR;
using Microsoft.Extensions.Configuration;
using OrdemCerta.Application.Abstractions;
using OrdemCerta.Domain.ServiceOrders.Events;
using OrdemCerta.Infrastructure.Repositories.CompanyRepository;
using OrdemCerta.Infrastructure.Repositories.CustomerRepository;

namespace OrdemCerta.Application.EventHandlers;

public class BudgetCreatedEventHandler : INotificationHandler<BudgetCreatedEvent>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IWhatsAppService _whatsAppService;
    private readonly string _baseUrl;

    public BudgetCreatedEventHandler(
        ICustomerRepository customerRepository,
        ICompanyRepository companyRepository,
        IWhatsAppService whatsAppService,
        IConfiguration configuration)
    {
        _customerRepository = customerRepository;
        _companyRepository = companyRepository;
        _whatsAppService = whatsAppService;
        _baseUrl = configuration["App:BaseUrl"] ?? string.Empty;
    }

    public async Task Handle(BudgetCreatedEvent notification, CancellationToken cancellationToken)
    {
        var customerResult = await _customerRepository.GetByIdAsync(notification.CustomerId, cancellationToken);
        if (customerResult.IsFailure || !customerResult.Value!.Phones.Any())
            return;

        var companyResult = await _companyRepository.GetByIdAsync(notification.CompanyId, cancellationToken);
        if (companyResult.IsFailure)
            return;

        var customer = customerResult.Value!;
        var company = companyResult.Value!;
        var phone = $"55{customer.Phones.First().Value}";

        var approveLink = $"{_baseUrl}/public/orders/{notification.ServiceOrderId}/approve";
        var refuseLink = $"{_baseUrl}/public/orders/{notification.ServiceOrderId}/refuse";

        var message = $"""
            🔧 *{company.Name.Value}*

            Olá, {customer.Name.FullName}! O orçamento da ordem *#{notification.OrderNumber}* está pronto.

            📱 *Aparelho:* {notification.DeviceType} {notification.Brand} {notification.Model}
            💰 *Valor:* R$ {notification.BudgetValue:N2}
            📋 *Descrição:* {notification.BudgetDescription}

            ✅ Aprovar: {approveLink}
            ❌ Recusar: {refuseLink}

            Dúvidas? Fale conosco: {company.Phone.GetFormatted()}
            """;

        await _whatsAppService.SendTextAsync(phone, message, cancellationToken);
    }
}
