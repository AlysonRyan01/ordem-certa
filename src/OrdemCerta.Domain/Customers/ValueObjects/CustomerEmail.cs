using OrdemCerta.Shared;

namespace OrdemCerta.Domain.Customers.ValueObjects;

public class CustomerEmail : ValueObject
{
    public string Value { get; private set; }

    private CustomerEmail(string value)
    {
        Value = value;
    }

    public static Result<CustomerEmail> Create(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return "E-mail é obrigatório";

        email = email.Trim().ToLowerInvariant();

        if (email.Length > 254)
            return "E-mail não pode ter mais de 254 caracteres";

        return new CustomerEmail(email);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}