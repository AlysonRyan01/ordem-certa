using OrdemCerta.Shared;

namespace OrdemCerta.Domain.Users.ValueObjects;

public class UserEmail : ValueObject
{
    public string Value { get; private set; }

    private UserEmail(string value)
    {
        Value = value;
    }

    public static Result<UserEmail> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "O e-mail é obrigatório";

        if (value.Length > 254)
            return "O e-mail deve ter no máximo 254 caracteres";

        return new UserEmail(value.Trim().ToLower());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
