using FluentValidation;
using OrdemCerta.Application.Inputs.UserInputs;

namespace OrdemCerta.Application.Validations.UserValidations;

public class RegisterUserInputValidator : AbstractValidator<RegisterUserInput>
{
    public RegisterUserInputValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("A empresa é obrigatória");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório")
            .MaximumLength(200).WithMessage("O nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório")
            .EmailAddress().WithMessage("E-mail inválido");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("A senha é obrigatória")
            .MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres");
    }
}
