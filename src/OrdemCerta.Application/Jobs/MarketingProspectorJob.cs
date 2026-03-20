using Microsoft.Extensions.Logging;
using OrdemCerta.Application.Abstractions;
using OrdemCerta.Domain.MarketingProspects;
using OrdemCerta.Infrastructure.DataContext.Uow;
using OrdemCerta.Infrastructure.Repositories.MarketingProspectRepository;

namespace OrdemCerta.Application.Jobs;

public class MarketingProspectorJob(
    IGooglePlacesService googlePlaces,
    IMarketingProspectRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<MarketingProspectorJob> logger)
{
    private const int MaxNewProspectsPerRun = 100;

    private static readonly string[] Cities =
    [
        "São Paulo SP", "Rio de Janeiro RJ", "Belo Horizonte MG", "Salvador BA",
        "Fortaleza CE", "Curitiba PR", "Manaus AM", "Recife PE", "Goiânia GO",
        "Belém PA", "Porto Alegre RS", "Campinas SP", "São Luís MA", "Maceió AL",
        "Natal RN", "Teresina PI", "Campo Grande MS", "João Pessoa PB",
        "Porto Velho RO", "Cuiabá MT", "Florianópolis SC", "Vitória ES",
        "Aracaju SE", "Macapá AP", "Palmas TO", "Rio Branco AC", "Boa Vista RR"
    ];

    private static readonly string[] SearchTerms =
    [
        "assistência técnica celular",
        "assistência técnica eletrônicos",
        "conserto notebook"
    ];

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("MarketingProspectorJob iniciado");

        var existingPlaceIds = await repository.GetAllPlaceIdsAsync(cancellationToken);
        var newCount = 0;

        foreach (var city in Cities)
        {
            if (newCount >= MaxNewProspectsPerRun) break;

            foreach (var term in SearchTerms)
            {
                if (newCount >= MaxNewProspectsPerRun) break;

                var query = $"{term} {city}";
                var results = await googlePlaces.SearchAsync(query, cancellationToken);

                foreach (var result in results)
                {
                    if (newCount >= MaxNewProspectsPerRun) break;
                    if (existingPlaceIds.Contains(result.PlaceId)) continue;

                    await Task.Delay(300, cancellationToken);

                    var details = await googlePlaces.GetDetailsAsync(result.PlaceId, cancellationToken);
                    if (details?.Phone == null)
                    {
                        existingPlaceIds.Add(result.PlaceId);
                        continue;
                    }

                    var prospect = MarketingProspect.Create(
                        result.PlaceId,
                        details.Name,
                        details.Phone,
                        details.City,
                        details.State);

                    await repository.AddAsync(prospect, cancellationToken);
                    await unitOfWork.CommitAsync(cancellationToken);

                    existingPlaceIds.Add(result.PlaceId);
                    newCount++;

                    logger.LogInformation("Novo prospect: {Name} ({City}/{State})", details.Name, details.City, details.State);
                }
            }
        }

        logger.LogInformation("MarketingProspectorJob finalizado. {Count} novo(s) prospect(s) adicionado(s)", newCount);
    }
}
