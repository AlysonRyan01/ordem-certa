using System.Text.Json.Serialization;

namespace OrdemCerta.Application.GooglePlaces;

public record PlaceSearchResult(string PlaceId, string Name);

public record PlaceDetails(string PlaceId, string Name, string? Phone, string? City, string? State);

// ── Raw API response models ──────────────────────────────────────────────────

internal class TextSearchResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = null!;

    [JsonPropertyName("results")]
    public List<TextSearchPlace> Results { get; set; } = [];

    [JsonPropertyName("next_page_token")]
    public string? NextPageToken { get; set; }
}

internal class TextSearchPlace
{
    [JsonPropertyName("place_id")]
    public string PlaceId { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
}

internal class PlaceDetailsResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = null!;

    [JsonPropertyName("result")]
    public PlaceDetailsResult? Result { get; set; }
}

internal class PlaceDetailsResult
{
    [JsonPropertyName("place_id")]
    public string PlaceId { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("international_phone_number")]
    public string? InternationalPhoneNumber { get; set; }

    [JsonPropertyName("address_components")]
    public List<AddressComponent> AddressComponents { get; set; } = [];
}

internal class AddressComponent
{
    [JsonPropertyName("long_name")]
    public string LongName { get; set; } = null!;

    [JsonPropertyName("types")]
    public List<string> Types { get; set; } = [];
}
