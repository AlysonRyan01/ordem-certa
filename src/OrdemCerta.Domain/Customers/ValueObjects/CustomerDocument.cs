using OrdemCerta.Shared;
using System.Text.RegularExpressions;
using OrdemCerta.Domain.Customers.Enums;

namespace OrdemCerta.Domain.Customers.ValueObjects;

public class CustomerDocument : ValueObject
{
    public string Value { get; private set; }
    public CustomerDocumentType Type { get; private set; }

    private CustomerDocument(string value, CustomerDocumentType type)
    {
        Value = value;
        Type = type;
    }

    public static Result<CustomerDocument> Create(string? document)
    {
        if (string.IsNullOrWhiteSpace(document))
            return "Documento é obrigatório";

        var cleanDocument = CleanDocument(document);

        if (cleanDocument.Length == 11)
        {
            if (!IsValidCpf(cleanDocument))
                return "CPF inválido";

            return new CustomerDocument(cleanDocument, CustomerDocumentType.Cpf);
        }

        if (cleanDocument.Length == 14)
        {
            if (!IsValidCnpj(cleanDocument))
                return "CNPJ inválido";

            return new CustomerDocument(cleanDocument, CustomerDocumentType.Cnpj);
        }

        return "Documento deve conter 11 dígitos (CPF) ou 14 dígitos (CNPJ)";
    }

    private static string CleanDocument(string document)
    {
        return Regex.Replace(document, @"[^\d]", string.Empty);
    }

    private static bool IsValidCpf(string cpf)
    {
        if (cpf.Length != 11)
            return false;

        if (cpf.Distinct().Count() == 1)
            return false;

        var sum = 0;
        for (var i = 0; i < 9; i++)
            sum += int.Parse(cpf[i].ToString()) * (10 - i);

        var remainder = sum % 11;
        var digit1 = remainder < 2 ? 0 : 11 - remainder;

        if (int.Parse(cpf[9].ToString()) != digit1)
            return false;

        sum = 0;
        for (var i = 0; i < 10; i++)
            sum += int.Parse(cpf[i].ToString()) * (11 - i);

        remainder = sum % 11;
        var digit2 = remainder < 2 ? 0 : 11 - remainder;

        return int.Parse(cpf[10].ToString()) == digit2;
    }

    private static bool IsValidCnpj(string cnpj)
    {
        if (cnpj.Length != 14)
            return false;

        if (cnpj.Distinct().Count() == 1)
            return false;

        var multipliers1 = new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        var sum = 0;
        for (var i = 0; i < 12; i++)
            sum += int.Parse(cnpj[i].ToString()) * multipliers1[i];

        var remainder = sum % 11;
        var digit1 = remainder < 2 ? 0 : 11 - remainder;

        if (int.Parse(cnpj[12].ToString()) != digit1)
            return false;

        var multipliers2 = new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        sum = 0;
        for (var i = 0; i < 13; i++)
            sum += int.Parse(cnpj[i].ToString()) * multipliers2[i];

        remainder = sum % 11;
        var digit2 = remainder < 2 ? 0 : 11 - remainder;

        return int.Parse(cnpj[13].ToString()) == digit2;
    }

    public string GetFormatted()
    {
        return Type == CustomerDocumentType.Cpf
            ? FormatCpf(Value)
            : FormatCnpj(Value);
    }

    private static string FormatCpf(string cpf)
    {
        return $"{cpf.Substring(0, 3)}.{cpf.Substring(3, 3)}.{cpf.Substring(6, 3)}-{cpf.Substring(9, 2)}";
    }

    private static string FormatCnpj(string cnpj)
    {
        return $"{cnpj.Substring(0, 2)}.{cnpj.Substring(2, 3)}.{cnpj.Substring(5, 3)}/{cnpj.Substring(8, 4)}-{cnpj.Substring(12, 2)}";
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return Type;
    }
}