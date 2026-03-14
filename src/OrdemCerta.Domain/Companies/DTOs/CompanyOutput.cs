namespace OrdemCerta.Domain.Companies.DTOs;

public class CompanyOutput
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Cnpj { get; set; }
    public string? CnpjFormatted { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string PhoneFormatted { get; set; } = string.Empty;
    public string? Street { get; set; }
    public string? Number { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string Plan { get; set; } = string.Empty;
}
