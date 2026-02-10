using OrdemCerta.Shared;

namespace OrdemCerta.Domain.Customers.ValueObjects;

public class CustomerAddress : ValueObject
{
    public string? Street { get; private set; }
    public string? Number { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }

    protected CustomerAddress(
        string? street, 
        string? number, 
        string? city, 
        string? state)
    {
        Street = street;
        Number = number;
        City = city;
        State = state;
    }

    public Result<CustomerAddress> Create(
        string? street, 
        string? number, 
        string? city, 
        string? state)
    {
        var customerAddress = new CustomerAddress(street, number, city, state);
        return customerAddress;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street ?? string.Empty;
        yield return Number ?? string.Empty;
        yield return City ?? string.Empty;
        yield return State ?? string.Empty;
    }
}