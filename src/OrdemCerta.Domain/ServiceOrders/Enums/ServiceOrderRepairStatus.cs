using System.Text.Json.Serialization;

namespace OrdemCerta.Domain.ServiceOrders.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ServiceOrderRepairStatus
{
    Entered = 1,
    Approved = 2,
    Disapproved = 3,
    Waiting = 4
}
