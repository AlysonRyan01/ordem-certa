using OrdemCerta.Domain.Customers.DTOs;

namespace OrdemCerta.Domain.Customers.Extensions;

public static class CustomerExtensions
{
    public static CustomerOutput ToOutput(this Customer customer)
    {
        return new CustomerOutput
        {
            Id = customer.Id,
            FullName = customer.FullName,
            Email = customer.Email,
            Phone = customer.Phone,
            PhoneFormatted = customer.GetPhoneFormatted(),
            Address = customer.AddressStreet != null || customer.AddressCity != null
                ? new CustomerAddressOutput
                {
                    Street = customer.AddressStreet,
                    Number = customer.AddressNumber,
                    City = customer.AddressCity,
                    State = customer.AddressState,
                    FullAddress = GetFullAddress(customer.AddressStreet, customer.AddressNumber, customer.AddressCity, customer.AddressState)
                }
                : null,
            Document = customer.Document != null ? new CustomerDocumentOutput
            {
                Value = customer.Document,
                Type = customer.DocumentType?.ToString(),
                Formatted = customer.GetDocumentFormatted()
            } : null
        };
    }

    public static List<CustomerOutput> ToOutput(this IEnumerable<Customer> customers)
    {
        return customers.Select(c => c.ToOutput()).ToList();
    }

    private static string GetFullAddress(string? street, string? number, string? city, string? state)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(street))
            parts.Add(street);

        if (!string.IsNullOrWhiteSpace(number))
            parts.Add(number);

        if (!string.IsNullOrWhiteSpace(city))
            parts.Add(city);

        if (!string.IsNullOrWhiteSpace(state))
            parts.Add(state);

        return string.Join(", ", parts);
    }
}
