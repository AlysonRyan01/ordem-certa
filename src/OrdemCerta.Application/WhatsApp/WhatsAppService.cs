using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrdemCerta.Application.Abstractions;

namespace OrdemCerta.Application.WhatsApp;

public class WhatsAppService : IWhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly string _instance;
    private readonly ILogger<WhatsAppService> _logger;

    public WhatsAppService(HttpClient httpClient, IConfiguration configuration, ILogger<WhatsAppService> logger)
    {
        _httpClient = httpClient;
        _instance = configuration["EvolutionApi:Instance"]
            ?? throw new InvalidOperationException("EvolutionApi:Instance não configurado");
        _logger = logger;
    }

    public async Task SendTextAsync(string phoneNumber, string message, CancellationToken cancellationToken)
    {
        var payload = new { number = phoneNumber, text = message };

        var response = await _httpClient.PostAsJsonAsync($"/message/sendText/{_instance}", payload, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Evolution API retornou {StatusCode} ao enviar para {Phone}. Resposta: {Body}",
                (int)response.StatusCode, phoneNumber, body);
            response.EnsureSuccessStatusCode();
        }

        _logger.LogInformation("WhatsApp enviado para {Phone}", phoneNumber);
    }
}
