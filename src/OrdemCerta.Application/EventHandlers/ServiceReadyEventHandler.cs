using MediatR;
using OrdemCerta.Application.Abstractions;
using OrdemCerta.Domain.ServiceOrders.Events;
using OrdemCerta.Infrastructure.Repositories.CompanyRepository;
using OrdemCerta.Infrastructure.Repositories.CustomerRepository;

namespace OrdemCerta.Application.EventHandlers;

public class ServiceReadyEventHandler : INotificationHandler<ServiceReadyEvent>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IWhatsAppService _whatsAppService;

    public ServiceReadyEventHandler(
        ICustomerRepository customerRepository,
        ICompanyRepository companyRepository,
        IWhatsAppService whatsAppService)
    {
        _customerRepository = customerRepository;
        _companyRepository = companyRepository;
        _whatsAppService = whatsAppService;
    }

    public async Task Handle(ServiceReadyEvent notification, CancellationToken cancellationToken)
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

        var message = $"""
            ✅ *Equipamento pronto para retirada!*

            Olá, {customer.Name.FullName}!

            A ordem *#{notification.OrderNumber}* — *{notification.DeviceType} {notification.Brand} {notification.Model}* — foi consertada e está pronta para retirada.

            📍 Venha buscar no nosso estabelecimento.
            📞 Dúvidas? Entre em contato: {company.Phone.GetFormatted()}

            *{company.Name.Value}*
            """;

        await _whatsAppService.SendTextAsync(phone, message, cancellationToken);
    }
}
