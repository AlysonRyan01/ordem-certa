using OrdemCerta.Shared;

namespace OrdemCerta.Domain.ServiceOrders.ValueObjects;

public class Budget : ValueObject
{
    public decimal Value { get; private set; }
    public string Description { get; private set; }

    private Budget(decimal value, string description)
    {
        Value = value;
        Description = description;
    }

    public static Result<Budget> Create(decimal value, string description)
    {
        if (value < 0)
            return "O valor do orçamento não pode ser negativo";

        if (string.IsNullOrWhiteSpace(description))
            return "A descrição do orçamento é obrigatória";

        if (description.Length > 500)
            return "A descrição deve ter no máximo 500 caracteres";

        return new Budget(value, description.Trim());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return Description;
    }
}
