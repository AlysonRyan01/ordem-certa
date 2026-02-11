using OrdemCerta.Domain.Customers.DTOs;
using OrdemCerta.Domain.Customers.ValueObjects;

namespace OrdemCerta.Domain.Customers.Extensions;

public static class CustomerExtensions
{
    public static CustomerOutput ToOutput(this Customer customer)
    {
        return new CustomerOutput
        {
            Id = customer.Id,
            FullName = customer.Name.FullName,
            Email = customer.Email?.Value,
            Phones = customer.Phones.Select(p => new CustomerPhoneOutput
            {
                Value = p.Value,
                AreaCode = p.AreaCode,
                Number = p.Number,
                Formatted = p.GetFormatted()
            }).ToList(),
            Address = customer.Address != null ? new CustomerAddressOutput
            {
                Street = customer.Address.Street,
                Number = customer.Address.Number,
                City = customer.Address.City,
                State = customer.Address.State,
                FullAddress = GetFullAddress(customer.Address)
            } : null,
            Document = customer.Document != null ? new CustomerDocumentOutput
            {
                Value = customer.Document.Value,
                Type = customer.Document.Type.ToString(),
                Formatted = customer.Document.GetFormatted()
            } : null
        };
    }

    public static List<CustomerOutput> ToOutput(this IEnumerable<Customer> customers)
    {
        return customers.Select(c => c.ToOutput()).ToList();
    }

    private static string GetFullAddress(CustomerAddress address)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(address.Street))
            parts.Add(address.Street);

        if (!string.IsNullOrWhiteSpace(address.Number))
            parts.Add(address.Number);

        if (!string.IsNullOrWhiteSpace(address.City))
            parts.Add(address.City);

        if (!string.IsNullOrWhiteSpace(address.State))
            parts.Add(address.State);

        return string.Join(", ", parts);
    }
}