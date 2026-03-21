using OrdemCerta.Domain.Customers.Enums;
using OrdemCerta.Shared;

namespace OrdemCerta.Domain.Customers;

public class Customer : AggregateRoot
{
    public Guid CompanyId { get; private set; }
    public string FullName { get; private set; } = null!;
    public List<CustomerPhone> Phones { get; private set; } = new();
    public string? Email { get; private set; }
    public string? AddressStreet { get; private set; }
    public string? AddressNumber { get; private set; }
    public string? AddressCity { get; private set; }
    public string? AddressState { get; private set; }
    public string? Document { get; private set; }
    public CustomerDocumentType? DocumentType { get; private set; }

    private Customer() { }

    private Customer(
        Guid companyId,
        string fullName,
        List<CustomerPhone> phones,
        string? email = null,
        string? addressStreet = null,
        string? addressNumber = null,
        string? addressCity = null,
        string? addressState = null,
        string? document = null,
        CustomerDocumentType? documentType = null)
    {
        Id = Guid.NewGuid();
        CompanyId = companyId;
        FullName = fullName;
        Email = email;
        Phones = phones;
        AddressStreet = addressStreet;
        AddressNumber = addressNumber;
        AddressCity = addressCity;
        AddressState = addressState;
        Document = document;
        DocumentType = documentType;
    }

    public static Result<Customer> Create(
        Guid companyId,
        string fullName,
        List<CustomerPhone> phones,
        string? email = null,
        string? addressStreet = null,
        string? addressNumber = null,
        string? addressCity = null,
        string? addressState = null,
        string? document = null,
        CustomerDocumentType? documentType = null)
    {
        return new Customer(companyId, fullName, phones, email, addressStreet, addressNumber, addressCity, addressState, document, documentType);
    }

    public Result AddPhone(CustomerPhone phone)
    {
        if (Phones.Any(p => p.Value == phone.Value))
            return Result.Failure("Este telefone já está cadastrado");

        Phones.Add(phone);
        return Result.Success();
    }

    public Result RemovePhone(CustomerPhone phone)
    {
        var existing = Phones.FirstOrDefault(p => p.Value == phone.Value);
        if (existing is null)
            return Result.Failure("Telefone não encontrado");

        Phones.Remove(existing);
        return Result.Success();
    }

    public Result UpdateEmail(string? email)
    {
        Email = email;
        return Result.Success();
    }

    public Result UpdateAddress(string? street, string? number, string? city, string? state)
    {
        AddressStreet = street;
        AddressNumber = number;
        AddressCity = city;
        AddressState = state;
        return Result.Success();
    }

    public Result UpdateDocument(string? document, CustomerDocumentType? documentType)
    {
        Document = document;
        DocumentType = documentType;
        return Result.Success();
    }

    public Result UpdateName(string fullName)
    {
        FullName = fullName;
        return Result.Success();
    }

    public string? GetDocumentFormatted()
    {
        if (Document is null || DocumentType is null) return null;
        return DocumentType == CustomerDocumentType.Cpf
            ? $"{Document[..3]}.{Document[3..6]}.{Document[6..9]}-{Document[9..]}"
            : $"{Document[..2]}.{Document[2..5]}.{Document[5..8]}/{Document[8..12]}-{Document[12..]}";
    }
}
