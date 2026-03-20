using System.Text.Json.Serialization;

namespace OrdemCerta.Domain.Sales.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SaleStatus
{
    Pending = 1,
    Completed = 2,
    Cancelled = 3
}
