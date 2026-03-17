using FluentValidation;
using OrdemCerta.Application.Inputs.ServiceOrderInputs;

namespace OrdemCerta.Application.Validations.ServiceOrderValidations;

public class SetWarrantyInputValidator : AbstractValidator<SetWarrantyInput>
{
    public SetWarrantyInputValidator()
    {
        RuleFor(x => x.Duration)
            .GreaterThan(0).WithMessage("O prazo da garantia deve ser maior que zero.");

        RuleFor(x => x.Unit)
            .IsInEnum().WithMessage("Unidade de garantia inválida.");
    }
}
