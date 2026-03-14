namespace OrdemCerta.Application.Inputs.CompanyInputs;

public record CreateCompanyInput(
    string Name,
    string Phone,
    string? Cnpj,
    string? Street,
    string? Number,
    string? City,
    string? State);
