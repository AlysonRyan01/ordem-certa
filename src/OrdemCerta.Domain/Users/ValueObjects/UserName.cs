using OrdemCerta.Shared;

namespace OrdemCerta.Domain.Users.ValueObjects;

public class UserName : ValueObject
{
    public string Value { get; private set; }

    private UserName(string value)
    {
        Value = value;
    }

    public static Result<UserName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "O nome é obrigatório";

        if (value.Length > 200)
            return "O nome deve ter no máximo 200 caracteres";

        return new UserName(value.Trim());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
