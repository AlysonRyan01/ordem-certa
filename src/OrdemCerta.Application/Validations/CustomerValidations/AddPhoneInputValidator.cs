using FluentValidation;
using OrdemCerta.Application.Inputs.CustomerInputs;

namespace OrdemCerta.Application.Validations.CustomerValidations;

public class AddPhoneInputValidator : AbstractValidator<AddPhoneInput>
{
    public AddPhoneInputValidator()
    {
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefone é obrigatório")
            .Matches(@"^\d{10,11}$").WithMessage("Telefone deve conter 10 ou 11 dígitos");
    }
}