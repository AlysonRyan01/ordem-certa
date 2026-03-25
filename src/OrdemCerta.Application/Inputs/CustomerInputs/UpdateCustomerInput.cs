namespace OrdemCerta.Application.Inputs.CustomerInputs;

public record UpdateCustomerInput(
    string FullName,
    string Phone,
    string? Email = null,
    string? Street = null,
    string? Number = null,
    string? City = null,
    string? State = null
);