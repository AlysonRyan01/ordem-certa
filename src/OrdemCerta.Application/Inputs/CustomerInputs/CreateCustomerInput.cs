namespace OrdemCerta.Application.Inputs.CustomerInputs;

public record CreateCustomerInput(
    string FullName,
    string Phone,
    string? Email = null,
    string? Document = null,
    string? Street = null,
    string? Number = null,
    string? City = null,
    string? State = null
);