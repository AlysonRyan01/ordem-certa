namespace OrdemCerta.Domain.Customers.DTOs;

public class CustomerOutput
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string PhoneFormatted { get; set; } = string.Empty;
    public CustomerAddressOutput? Address { get; set; }
    public CustomerDocumentOutput? Document { get; set; }
}