using OrdemCerta.Shared;
using System.Text.RegularExpressions;

namespace OrdemCerta.Domain.Customers.ValueObjects;

public class CustomerPhone : ValueObject
{
    public string Value { get; private set; }
    public string AreaCode { get; private set; }
    public string Number { get; private set; }

    private CustomerPhone(string value, string areaCode, string number)
    {
        Value = value;
        AreaCode = areaCode;
        Number = number;
    }

    public static Result<CustomerPhone> Create(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return "Telefone é obrigatório";

        var cleanPhone = CleanPhone(phone);

        if (cleanPhone.Length == 11)
        {
            var areaCode = cleanPhone.Substring(0, 2);
            var number = cleanPhone.Substring(2);

            if (number[0] != '9')
                return "Telefone celular deve começar com 9";

            return new CustomerPhone(cleanPhone, areaCode, number);
        }

        if (cleanPhone.Length == 10)
        {
            var areaCode = cleanPhone.Substring(0, 2);
            var number = cleanPhone.Substring(2);

            if (number[0] == '9')
                return "Telefone fixo não deve começar com 9";

            return new CustomerPhone(cleanPhone, areaCode, number);
        }

        return "Telefone deve conter 10 dígitos (fixo) ou 11 dígitos (celular)";
    }

    private static string CleanPhone(string phone)
    {
        return Regex.Replace(phone, @"[^\d]", string.Empty);
    }

    public string GetFormatted()
    {
        if (Value.Length == 11)
            return $"({AreaCode}) {Number.Substring(0, 5)}-{Number.Substring(5)}";

        return $"({AreaCode}) {Number.Substring(0, 4)}-{Number.Substring(4)}";
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}