using OrdemCerta.Application.GooglePlaces;

namespace OrdemCerta.Application.Abstractions;

public interface IGooglePlacesService
{
    Task<List<PlaceSearchResult>> SearchAsync(string query, CancellationToken cancellationToken);
    Task<PlaceDetails?> GetDetailsAsync(string placeId, CancellationToken cancellationToken);
}
