using MediatR;
using OrdemCerta.Application.Abstractions;
using OrdemCerta.Domain.ServiceOrders.Events;
using OrdemCerta.Infrastructure.Repositories.CompanyRepository;

namespace OrdemCerta.Application.EventHandlers;

public class BudgetRespondedEventHandler : INotificationHandler<BudgetRespondedEvent>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IWhatsAppService _whatsAppService;

    public BudgetRespondedEventHandler(
        ICompanyRepository companyRepository,
        IWhatsAppService whatsAppService)
    {
        _companyRepository = companyRepository;
        _whatsAppService = whatsAppService;
    }

    public async Task Handle(BudgetRespondedEvent notification, CancellationToken cancellationToken)
    {
        var companyResult = await _companyRepository.GetByIdAsync(notification.CompanyId, cancellationToken);
        if (companyResult.IsFailure)
            return;

        var company = companyResult.Value!;
        var phone = $"55{company.Phone.Value}";

        var status = notification.Approved ? "✅ APROVADO" : "❌ RECUSADO";
        var message = $"""
            {status}

            O cliente respondeu ao orçamento da ordem #*{notification.OrderNumber}*.

            Acesse o sistema para mais detalhes.
            """;

        await _whatsAppService.SendTextAsync(phone, message, cancellationToken);
    }
}
