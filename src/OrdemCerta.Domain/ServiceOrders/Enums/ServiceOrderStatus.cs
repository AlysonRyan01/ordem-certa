using System.Text.Json.Serialization;

namespace OrdemCerta.Domain.ServiceOrders.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ServiceOrderStatus
{
    Received = 1,
    UnderAnalysis = 2,
    BudgetPending = 3,
    WaitingApproval = 4,
    BudgetApproved = 5,
    BudgetRefused = 6,
    UnderRepair = 7,
    ReadyForPickup = 8,
    Delivered = 9,
    Cancelled = 10
}
