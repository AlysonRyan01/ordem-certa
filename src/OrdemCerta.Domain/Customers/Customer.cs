using OrdemCerta.Domain.Customers.Enums;
using OrdemCerta.Shared;

namespace OrdemCerta.Domain.Customers;

public class Customer : AggregateRoot
{
    public Guid CompanyId { get; private set; }
    public string FullName { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public string PhoneAreaCode { get; private set; } = null!;
    public string PhoneNumber { get; private set; } = null!;
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
        string phone,
        string phoneAreaCode,
        string phoneNumber,
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
        Phone = phone;
        PhoneAreaCode = phoneAreaCode;
        PhoneNumber = phoneNumber;
        Email = email;
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
        string phone,
        string phoneAreaCode,
        string phoneNumber,
        string? email = null,
        string? addressStreet = null,
        string? addressNumber = null,
        string? addressCity = null,
        string? addressState = null,
        string? document = null,
        CustomerDocumentType? documentType = null)
    {
        return new Customer(companyId, fullName, phone, phoneAreaCode, phoneNumber, email, addressStreet, addressNumber, addressCity, addressState, document, documentType);
    }

    public Result UpdatePhone(string phone, string areaCode, string number)
    {
        Phone = phone;
        PhoneAreaCode = areaCode;
        PhoneNumber = number;
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

    public string GetPhoneFormatted()
    {
        if (Phone.Length == 11)
            return $"({PhoneAreaCode}) {PhoneNumber[..5]}-{PhoneNumber[5..]}";

        return $"({PhoneAreaCode}) {PhoneNumber[..4]}-{PhoneNumber[4..]}";
    }

    public string? GetDocumentFormatted()
    {
        if (Document is null || DocumentType is null) return null;
        return DocumentType == CustomerDocumentType.Cpf
            ? $"{Document[..3]}.{Document[3..6]}.{Document[6..9]}-{Document[9..]}"
            : $"{Document[..2]}.{Document[2..5]}.{Document[5..8]}/{Document[8..12]}-{Document[12..]}";
    }
}
