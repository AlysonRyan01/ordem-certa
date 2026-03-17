using OrdemCerta.Domain.ServiceOrders.Enums;
using OrdemCerta.Shared;

namespace OrdemCerta.Domain.ServiceOrders.ValueObjects;

public class Warranty : ValueObject
{
    public int Duration { get; private set; }
    public WarrantyUnit Unit { get; private set; }

    private Warranty(int duration, WarrantyUnit unit)
    {
        Duration = duration;
        Unit = unit;
    }

    public static Result<Warranty> Create(int duration, WarrantyUnit unit)
    {
        if (duration <= 0)
            return "O prazo da garantia deve ser maior que zero.";

        if (!Enum.IsDefined(typeof(WarrantyUnit), unit))
            return "Unidade de garantia inválida.";

        return new Warranty(duration, unit);
    }

    public string GetFormatted()
    {
        var label = Unit switch
        {
            WarrantyUnit.Days   => Duration == 1 ? "dia"  : "dias",
            WarrantyUnit.Months => Duration == 1 ? "mês"  : "meses",
            WarrantyUnit.Years  => Duration == 1 ? "ano"  : "anos",
            _                   => Unit.ToString()
        };
        return $"{Duration} {label}";
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Duration;
        yield return Unit;
    }
}
