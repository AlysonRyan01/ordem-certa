namespace OrdemCerta.Application.Inputs.CompanyInputs;

public record ConfirmPasswordResetInput(string Email, string Code, string NewPassword);
