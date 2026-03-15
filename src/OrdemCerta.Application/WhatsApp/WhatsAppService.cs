using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using OrdemCerta.Application.Abstractions;

namespace OrdemCerta.Application.WhatsApp;

public class WhatsAppService : IWhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly string _instance;

    public WhatsAppService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _instance = configuration["EvolutionApi:Instance"]
            ?? throw new InvalidOperationException("EvolutionApi:Instance não configurado");
    }

    public async Task SendTextAsync(string phoneNumber, string message, CancellationToken cancellationToken)
    {
        var payload = new { number = phoneNumber, text = message };
        await _httpClient.PostAsJsonAsync($"/message/sendText/{_instance}", payload, cancellationToken);
    }
}
