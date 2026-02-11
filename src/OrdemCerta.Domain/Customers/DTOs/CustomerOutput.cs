namespace OrdemCerta.Domain.Customers.DTOs;

public class CustomerOutput
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public List<CustomerPhoneOutput> Phones { get; set; } = new();
    public CustomerAddressOutput? Address { get; set; }
    public CustomerDocumentOutput? Document { get; set; }
}