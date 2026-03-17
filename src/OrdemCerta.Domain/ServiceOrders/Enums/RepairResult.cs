using System.Text.Json.Serialization;

namespace OrdemCerta.Domain.ServiceOrders.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RepairResult
{
    CanBeRepaired = 1,
    NoFix = 2,
    NoDefectFound = 3
}
