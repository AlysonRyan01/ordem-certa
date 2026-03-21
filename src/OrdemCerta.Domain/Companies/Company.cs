using OrdemCerta.Domain.Companies.Enums;
using OrdemCerta.Shared;

namespace OrdemCerta.Domain.Companies;

public class Company : AggregateRoot
{
    public string Name { get; private set; } = null!;
    public string? Cnpj { get; private set; }
    public string Phone { get; private set; } = null!;
    public string PhoneAreaCode { get; private set; } = null!;
    public string PhoneNumber { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string? Street { get; private set; }
    public string? Number { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }
    public PlanType Plan { get; private set; }
    public string? StripeCustomerId { get; private set; }
    public string? StripeSubscriptionId { get; private set; }

    protected Company() { }

    private Company(
        string name,
        string phone,
        string phoneAreaCode,
        string phoneNumber,
        string email,
        string passwordHash,
        PlanType plan,
        string? cnpj = null,
        string? street = null,
        string? number = null,
        string? city = null,
        string? state = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        Cnpj = cnpj;
        Phone = phone;
        PhoneAreaCode = phoneAreaCode;
        PhoneNumber = phoneNumber;
        Email = email;
        PasswordHash = passwordHash;
        Plan = plan;
        Street = street;
        Number = number;
        City = city;
        State = state;
    }

    public static Result<Company> Create(
        string name,
        string phone,
        string phoneAreaCode,
        string phoneNumber,
        string email,
        string passwordHash,
        string? cnpj = null,
        string? street = null,
        string? number = null,
        string? city = null,
        string? state = null)
    {
        return new Company(name, phone, phoneAreaCode, phoneNumber, email, passwordHash, PlanType.Demo, cnpj, street, number, city, state);
    }

    public Result UpdateName(string name)
    {
        Name = name;
        return Result.Success();
    }

    public Result UpdatePhone(string phone, string phoneAreaCode, string phoneNumber)
    {
        Phone = phone;
        PhoneAreaCode = phoneAreaCode;
        PhoneNumber = phoneNumber;
        return Result.Success();
    }

    public Result UpdateAddress(string? street, string? number, string? city, string? state)
    {
        Street = street;
        Number = number;
        City = city;
        State = state;
        return Result.Success();
    }

    public Result UpdatePasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
        return Result.Success();
    }

    public Result SetStripeCustomerId(string stripeCustomerId)
    {
        StripeCustomerId = stripeCustomerId;
        return Result.Success();
    }

    public Result ActivateSubscription(string stripeSubscriptionId)
    {
        StripeSubscriptionId = stripeSubscriptionId;
        Plan = PlanType.Paid;
        return Result.Success();
    }

    public Result CancelSubscription()
    {
        StripeSubscriptionId = null;
        Plan = PlanType.Demo;
        return Result.Success();
    }

    public string GetPhoneFormatted()
    {
        return PhoneNumber.Length == 9
            ? $"({PhoneAreaCode}) {PhoneNumber[..5]}-{PhoneNumber[5..]}"
            : $"({PhoneAreaCode}) {PhoneNumber[..4]}-{PhoneNumber[4..]}";
    }

    public string? GetCnpjFormatted()
    {
        if (Cnpj is null || Cnpj.Length != 14) return Cnpj;
        return $"{Cnpj[..2]}.{Cnpj[2..5]}.{Cnpj[5..8]}/{Cnpj[8..12]}-{Cnpj[12..]}";
    }
}
