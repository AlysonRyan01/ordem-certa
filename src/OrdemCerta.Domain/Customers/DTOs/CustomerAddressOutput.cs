namespace OrdemCerta.Domain.Customers.DTOs;

public class CustomerAddressOutput
{
    public string? Street { get; set; }
    public string? Number { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string FullAddress { get; set; } = string.Empty;
}