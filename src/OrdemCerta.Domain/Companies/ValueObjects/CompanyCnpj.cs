using System.Text.RegularExpressions;
using OrdemCerta.Shared;

namespace OrdemCerta.Domain.Companies.ValueObjects;

public class CompanyCnpj : ValueObject
{
    public string Value { get; private set; }

    private CompanyCnpj(string value)
    {
        Value = value;
    }

    public static Result<CompanyCnpj> Create(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return "O CNPJ é obrigatório";

        var digits = Regex.Replace(cnpj, @"\D", "");

        if (digits.Length != 14)
            return "CNPJ inválido";

        if (digits.Distinct().Count() == 1)
            return "CNPJ inválido";

        if (!IsValidCnpj(digits))
            return "CNPJ inválido";

        return new CompanyCnpj(digits);
    }

    public string GetFormatted()
    {
        return $"{Value[..2]}.{Value[2..5]}.{Value[5..8]}/{Value[8..12]}-{Value[12..]}";
    }

    private static bool IsValidCnpj(string digits)
    {
        int[] weights1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] weights2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        var sum = digits.Take(12).Select((d, i) => (d - '0') * weights1[i]).Sum();
        var remainder = sum % 11;
        var digit1 = remainder < 2 ? 0 : 11 - remainder;

        if ((digits[12] - '0') != digit1)
            return false;

        sum = digits.Take(13).Select((d, i) => (d - '0') * weights2[i]).Sum();
        remainder = sum % 11;
        var digit2 = remainder < 2 ? 0 : 11 - remainder;

        return (digits[13] - '0') == digit2;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
