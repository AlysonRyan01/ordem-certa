using Microsoft.Extensions.Logging;
using OrdemCerta.Application.Abstractions;
using OrdemCerta.Infrastructure.DataContext.Uow;
using OrdemCerta.Infrastructure.Repositories.MarketingProspectRepository;

namespace OrdemCerta.Application.Jobs;

public class MarketingDispatcherJob(
    IMarketingProspectRepository repository,
    IWhatsAppService whatsAppService,
    IUnitOfWork unitOfWork,
    ILogger<MarketingDispatcherJob> logger)
{
    private const int BatchSize = 1;
    private const string LinkMessage = "ordemcerta.app";

    private static string BuildMessage(string companyName) =>
        $"Olá! Tudo bem?\n\n" +
        $"Vi a *{companyName}* e queria apresentar uma ferramenta que pode facilitar o dia a dia de vocês: o *OrdemCerta*.\n\n" +
        $"Com ele você gerencia ordens de serviço e envia orçamentos automatizados pelo próprio WhatsApp — o cliente aprova na hora, sem precisar ligar.\n\n" +
        $"É gratuito para começar e qualquer dúvida é só responder aqui!";

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var prospects = await repository.GetPendingAsync(BatchSize, cancellationToken);

        if (prospects.Count == 0)
        {
            logger.LogInformation("MarketingDispatcherJob: nenhum prospect pendente");
            return;
        }

        logger.LogInformation("MarketingDispatcherJob: enviando para {Count} prospect(s)", prospects.Count);

        foreach (var prospect in prospects)
        {
            try
            {
                await whatsAppService.SendTextAsync(prospect.PhoneNumber, BuildMessage(prospect.BusinessName), cancellationToken);

                await Task.Delay(1500, cancellationToken);

                await whatsAppService.SendTextAsync(prospect.PhoneNumber, LinkMessage, cancellationToken);

                prospect.MarkContacted();
                logger.LogInformation("Mensagem enviada para {Name} ({Phone})", prospect.BusinessName, prospect.PhoneNumber);
            }
            catch (Exception ex)
            {
                prospect.MarkFailed();
                logger.LogWarning(ex, "Falha ao enviar para {Name} ({Phone})", prospect.BusinessName, prospect.PhoneNumber);
            }

            await repository.UpdateAsync(prospect, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            await Task.Delay(3000, cancellationToken);
        }
    }
}
