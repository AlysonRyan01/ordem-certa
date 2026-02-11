using OrdemCerta.Shared;

namespace OrdemCerta.Domain.Customers.ValueObjects;

public class CustomerName : ValueObject
{
    public string FullName { get; private set; }

    private CustomerName(string fullName)
    {
        FullName = fullName;
    }

    public Result<CustomerName> Create(string fullName)
    {
        var customerName = new CustomerName(fullName);
        return customerName;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FullName;
    }
}