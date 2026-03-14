namespace OrdemCerta.Application.Inputs.CompanyInputs;

public record UpdateCompanyInput(
    string Name,
    string Phone,
    string? Street,
    string? Number,
    string? City,
    string? State);
