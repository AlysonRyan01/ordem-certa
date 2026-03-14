using OrdemCerta.Shared;

namespace OrdemCerta.Domain.Companies.ValueObjects;

public class CompanyName : ValueObject
{
    public string Value { get; private set; }

    private CompanyName(string value)
    {
        Value = value;
    }

    public static Result<CompanyName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "O nome da empresa é obrigatório";

        if (value.Length > 200)
            return "O nome da empresa deve ter no máximo 200 caracteres";

        return new CompanyName(value.Trim());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
