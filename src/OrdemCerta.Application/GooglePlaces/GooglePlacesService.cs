using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrdemCerta.Application.Abstractions;

namespace OrdemCerta.Application.GooglePlaces;

public partial class GooglePlacesService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<GooglePlacesService> logger) : IGooglePlacesService
{
    private readonly string _apiKey = configuration["GooglePlaces:ApiKey"]
        ?? throw new InvalidOperationException("GooglePlaces:ApiKey não configurado");

    public async Task<List<PlaceSearchResult>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        var url = $"https://maps.googleapis.com/maps/api/place/textsearch/json?query={Uri.EscapeDataString(query)}&language=pt-BR&key={_apiKey}";

        try
        {
            var response = await httpClient.GetFromJsonAsync<TextSearchResponse>(url, cancellationToken);

            if (response?.Status != "OK" && response?.Status != "ZERO_RESULTS")
                logger.LogWarning("Google Places TextSearch retornou status {Status} para query '{Query}'", response?.Status, query);

            return response?.Results.Select(r => new PlaceSearchResult(r.PlaceId, r.Name)).ToList() ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao buscar no Google Places para query '{Query}'", query);
            return [];
        }
    }

    public async Task<PlaceDetails?> GetDetailsAsync(string placeId, CancellationToken cancellationToken)
    {
        var fields = "place_id,name,international_phone_number,address_components";
        var url = $"https://maps.googleapis.com/maps/api/place/details/json?place_id={placeId}&fields={fields}&language=pt-BR&key={_apiKey}";

        try
        {
            var response = await httpClient.GetFromJsonAsync<PlaceDetailsResponse>(url, cancellationToken);

            if (response?.Status != "OK" || response.Result == null)
                return null;

            var result = response.Result;
            var phone = FormatBrazilianPhone(result.InternationalPhoneNumber);
            if (phone == null) return null;

            var city = result.AddressComponents
                .FirstOrDefault(c => c.Types.Contains("administrative_area_level_2"))?.LongName;

            var state = result.AddressComponents
                .FirstOrDefault(c => c.Types.Contains("administrative_area_level_1"))?.LongName;

            return new PlaceDetails(placeId, result.Name, phone, city, state);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao buscar detalhes do place_id '{PlaceId}'", placeId);
            return null;
        }
    }

    private static string? FormatBrazilianPhone(string? internationalPhone)
    {
        if (string.IsNullOrWhiteSpace(internationalPhone)) return null;

        // Remove tudo exceto dígitos
        var digits = NonDigitRegex().Replace(internationalPhone, "");

        // Deve começar com 55 (Brasil) e ter 12 ou 13 dígitos
        if (!digits.StartsWith("55") || digits.Length < 12 || digits.Length > 13)
            return null;

        return digits;
    }

    [GeneratedRegex(@"\D")]
    private static partial Regex NonDigitRegex();
}
