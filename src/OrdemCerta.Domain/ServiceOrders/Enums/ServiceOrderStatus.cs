using System.Text.Json.Serialization;

namespace OrdemCerta.Domain.ServiceOrders.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ServiceOrderStatus
{
    UnderAnalysis = 1,
    BudgetPending = 2,
    UnderRepair = 3,
    ReadyForPickup = 4,
    Delivered = 5,
    Cancelled = 6
}
