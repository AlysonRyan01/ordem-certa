using FluentValidation;
using OrdemCerta.Application.Inputs.UserInputs;

namespace OrdemCerta.Application.Validations.UserValidations;

public class LoginInputValidator : AbstractValidator<LoginInput>
{
    public LoginInputValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("A senha é obrigatória");
    }
}
