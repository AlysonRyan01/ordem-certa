using FluentValidation;
using OrdemCerta.Application.Inputs.CustomerInputs;

namespace OrdemCerta.Application.Validations.CustomerValidations;

public class RemovePhoneInputValidator : AbstractValidator<RemovePhoneInput>
{
    public RemovePhoneInputValidator()
    {
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefone é obrigatório");
    }
}