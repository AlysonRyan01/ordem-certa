using OrdemCerta.Application.Abstractions;

namespace OrdemCerta.Application.WhatsApp;

public class WhatsAppJobs(IWhatsAppService whatsAppService)
{
    public async Task SendTextAsync(string phoneNumber, string message, CancellationToken cancellationToken)
    {
        await whatsAppService.SendTextAsync(phoneNumber, message, cancellationToken);
    }
}
