using OrdemCerta.Domain.Companies.Enums;
using OrdemCerta.Domain.Companies.ValueObjects;
using OrdemCerta.Shared;

namespace OrdemCerta.Domain.Companies;

public class Company : AggregateRoot
{
    public CompanyName Name { get; private set; } = null!;
    public CompanyCnpj? Cnpj { get; private set; }
    public CompanyPhone Phone { get; private set; } = null!;
    public string? Street { get; private set; }
    public string? Number { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }
    public PlanType Plan { get; private set; }

    protected Company() { }

    private Company(
        CompanyName name,
        CompanyPhone phone,
        PlanType plan,
        CompanyCnpj? cnpj = null,
        string? street = null,
        string? number = null,
        string? city = null,
        string? state = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        Cnpj = cnpj;
        Phone = phone;
        Plan = plan;
        Street = street;
        Number = number;
        City = city;
        State = state;
    }

    public static Result<Company> Create(
        CompanyName name,
        CompanyPhone phone,
        CompanyCnpj? cnpj = null,
        string? street = null,
        string? number = null,
        string? city = null,
        string? state = null)
    {
        var company = new Company(name, phone, PlanType.Demo, cnpj, street, number, city, state);
        return company;
    }

    public Result UpdateName(CompanyName name)
    {
        Name = name;
        return Result.Success();
    }

    public Result UpdatePhone(CompanyPhone phone)
    {
        Phone = phone;
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

    public Result UpgradeToPaid()
    {
        Plan = PlanType.Paid;
        return Result.Success();
    }
}
