using OrdemCerta.Domain.MarketingProspects.Enums;
using OrdemCerta.Shared;

namespace OrdemCerta.Domain.MarketingProspects;

public class MarketingProspect : AggregateRoot
{
    public string PlaceId { get; private set; } = null!;
    public string BusinessName { get; private set; } = null!;
    public string PhoneNumber { get; private set; } = null!;
    public string? City { get; private set; }
    public string? State { get; private set; }
    public ProspectStatus Status { get; private set; }
    public DateTime? ContactedAt { get; private set; }

    protected MarketingProspect() { }

    public static MarketingProspect Create(
        string placeId,
        string businessName,
        string phoneNumber,
        string? city,
        string? state)
    {
        return new MarketingProspect
        {
            Id = Guid.NewGuid(),
            PlaceId = placeId,
            BusinessName = businessName,
            PhoneNumber = phoneNumber,
            City = city,
            State = state,
            Status = ProspectStatus.Pending
        };
    }

    public void MarkContacted()
    {
        Status = ProspectStatus.Contacted;
        ContactedAt = DateTime.UtcNow;
    }

    public void MarkFailed()
    {
        Status = ProspectStatus.Failed;
    }
}
