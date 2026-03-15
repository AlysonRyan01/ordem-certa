namespace OrdemCerta.Application.Abstractions;

public interface IWhatsAppService
{
    Task SendTextAsync(string phoneNumber, string message, CancellationToken cancellationToken);
}
