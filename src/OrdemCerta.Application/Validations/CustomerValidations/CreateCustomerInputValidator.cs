using FluentValidation;
using OrdemCerta.Application.Inputs.CustomerInputs;

namespace OrdemCerta.Application.Validations.CustomerValidations;

public class CreateCustomerInputValidator : AbstractValidator<CreateCustomerInput>
{
    public CreateCustomerInputValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefone é obrigatório")
            .Matches(@"^\d{10,11}$").WithMessage("Telefone deve conter 10 ou 11 dígitos");

        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("E-mail inválido")
                .MaximumLength(254).WithMessage("E-mail deve ter no máximo 254 caracteres");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Document), () =>
        {
            RuleFor(x => x.Document)
                .Must(BeValidCpfOrCnpj).WithMessage("Documento deve ser um CPF (11 dígitos) ou CNPJ (14 dígitos) válido");
        });

        When(x => !string.IsNullOrWhiteSpace(x.State), () =>
        {
            RuleFor(x => x.State)
                .Length(2).WithMessage("Estado deve ter 2 caracteres")
                .Matches(@"^[A-Z]{2}$").WithMessage("Estado deve conter apenas letras maiúsculas");
        });
    }

    private bool BeValidCpfOrCnpj(string? document)
    {
        if (string.IsNullOrWhiteSpace(document))
            return false;

        var cleanDocument = new string(document.Where(char.IsDigit).ToArray());
        return cleanDocument.Length == 11 || cleanDocument.Length == 14;
    }
}