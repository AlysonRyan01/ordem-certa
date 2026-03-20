using System.Text.Json.Serialization;

namespace OrdemCerta.Domain.Sales.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SalePaymentMethod
{
    Cash = 1,
    CreditCard = 2,
    DebitCard = 3,
    Pix = 4,
    BankTransfer = 5,
    Check = 6,
    Other = 7
}
