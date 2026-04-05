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
    private const int TermsPerRun = 5;

    private static readonly string[] Cities =
    [
        // Capitais
        "São Paulo SP", "Rio de Janeiro RJ", "Belo Horizonte MG", "Salvador BA",
        "Fortaleza CE", "Curitiba PR", "Manaus AM", "Recife PE", "Goiânia GO",
        "Belém PA", "Porto Alegre RS", "Campinas SP", "São Luís MA", "Maceió AL",
        "Natal RN", "Teresina PI", "Campo Grande MS", "João Pessoa PB",
        "Porto Velho RO", "Cuiabá MT", "Florianópolis SC", "Vitória ES",
        "Aracaju SE", "Macapá AP", "Palmas TO", "Rio Branco AC", "Boa Vista RR",
        // Cidades médias
        "Ribeirão Preto SP", "Sorocaba SP", "São Bernardo do Campo SP",
        "Santo André SP", "Osasco SP", "Guarulhos SP", "Mauá SP",
        "Uberlândia MG", "Contagem MG", "Juiz de Fora MG", "Montes Claros MG",
        "Feira de Santana BA", "Vitória da Conquista BA", "Camaçari BA",
        "Joinville SC", "Blumenau SC", "Chapecó SC",
        "Londrina PR", "Maringá PR", "Ponta Grossa PR",
        "Caxias do Sul RS", "Pelotas RS", "Santa Maria RS",
        "Niterói RJ", "Duque de Caxias RJ", "Nova Iguaçu RJ",
        "Caucaia CE", "Juazeiro do Norte CE", "Sobral CE",
        "Caruaru PE", "Petrolina PE", "Olinda PE",
        "Aparecida de Goiânia GO", "Anápolis GO",
        "Imperatriz MA", "Timon MA",
        "Mossoró RN", "Parnamirim RN",
        "Campina Grande PB",
        "Santarém PA", "Marabá PA",
        "Porto Velho RO", "Ji-Paraná RO",
        "Sinop MT", "Várzea Grande MT"
    ];

    private static readonly string[] SearchTermsPool =
    [
        "assistência técnica celular",
        "assistência técnica eletrônicos",
        "conserto notebook",
        "conserto de celular",
        "reparo smartphone",
        "assistência técnica iPhone",
        "manutenção eletrônicos",
        "técnico de celular",
        "conserto Samsung",
        "troca de tela celular",
        "reparo notebook",
        "assistência técnica computador",
        "conserto tablet",
        "assistência técnica Apple",
        "reparo de computador",
        "técnico em informática",
        "loja conserto celular",
        "celular com defeito conserto"
    ];

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("MarketingProspectorJob iniciado");

        var existingPlaceIds = await repository.GetAllPlaceIdsAsync(cancellationToken);
        var newCount = 0;

        var cities = Cities.OrderBy(_ => Random.Shared.Next()).ToArray();
        var terms = SearchTermsPool.OrderBy(_ => Random.Shared.Next()).Take(TermsPerRun).ToArray();

        logger.LogInformation("Termos desta rodada: {Terms}", string.Join(", ", terms));

        foreach (var city in cities)
        {
            if (newCount >= MaxNewProspectsPerRun) break;

            foreach (var term in terms)
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
