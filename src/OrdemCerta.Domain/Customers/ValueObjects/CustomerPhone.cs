using System.Text.RegularExpressions;
using OrdemCerta.Shared;

namespace OrdemCerta.Domain.Customers;

public class CustomerPhone
{
    public int Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public string Value { get; private set; } = null!;
    public string AreaCode { get; private set; } = null!;
    public string Number { get; private set; } = null!;

    protected CustomerPhone() { }

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

        var cleanPhone = Regex.Replace(phone, @"[^\d]", string.Empty);

        if (cleanPhone.Length == 11)
        {
            var areaCode = cleanPhone[..2];
            var number = cleanPhone[2..];

            if (number[0] != '9')
                return "Telefone celular deve começar com 9";

            return new CustomerPhone(cleanPhone, areaCode, number);
        }

        if (cleanPhone.Length == 10)
        {
            var areaCode = cleanPhone[..2];
            var number = cleanPhone[2..];

            if (number[0] == '9')
                return "Telefone fixo não deve começar com 9";

            return new CustomerPhone(cleanPhone, areaCode, number);
        }

        return "Telefone deve conter 10 dígitos (fixo) ou 11 dígitos (celular)";
    }

    public string GetFormatted()
    {
        if (Value.Length == 11)
            return $"({AreaCode}) {Number[..5]}-{Number[5..]}";

        return $"({AreaCode}) {Number[..4]}-{Number[4..]}";
    }
}
