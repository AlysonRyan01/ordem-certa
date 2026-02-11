namespace OrdemCerta.Domain.Customers.DTOs;

public class CustomerPhoneOutput
{
    public string Value { get; set; } = string.Empty;
    public string AreaCode { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Formatted { get; set; } = string.Empty;
}