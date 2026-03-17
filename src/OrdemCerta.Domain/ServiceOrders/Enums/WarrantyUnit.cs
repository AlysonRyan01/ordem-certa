using System.Text.Json.Serialization;

namespace OrdemCerta.Domain.ServiceOrders.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WarrantyUnit
{
    Days = 1,
    Months = 2,
    Years = 3
}
