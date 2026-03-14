using System.Text.RegularExpressions;
using OrdemCerta.Shared;

namespace OrdemCerta.Domain.Companies.ValueObjects;

public class CompanyPhone : ValueObject
{
    public string Value { get; private set; }
    public string AreaCode { get; private set; }
    public string Number { get; private set; }

    private CompanyPhone(string value, string areaCode, string number)
    {
        Value = value;
        AreaCode = areaCode;
        Number = number;
    }

    public static Result<CompanyPhone> Create(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return "O telefone é obrigatório";

        var digits = Regex.Replace(phone, @"\D", "");

        if (digits.Length < 10 || digits.Length > 11)
            return "Telefone inválido. Informe DDD + número (10 ou 11 dígitos)";

        var areaCode = digits[..2];
        var number = digits[2..];

        if (number.Length == 11 && number[0] != '9')
            return "Celular deve começar com 9";

        if (number.Length == 10 && number[0] == '9')
            return "Telefone fixo não deve começar com 9";

        return new CompanyPhone(digits, areaCode, number);
    }

    public string GetFormatted()
    {
        return Number.Length == 9
            ? $"({AreaCode}) {Number[..5]}-{Number[5..]}"
            : $"({AreaCode}) {Number[..4]}-{Number[4..]}";
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
