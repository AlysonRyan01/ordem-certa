namespace OrdemCerta.Application.Inputs.UserInputs;

public record RegisterUserInput(
    Guid CompanyId,
    string Name,
    string Email,
    string Password);
