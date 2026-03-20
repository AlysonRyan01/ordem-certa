using FluentValidation;
using OrdemCerta.Application.Inputs.SaleInputs;

namespace OrdemCerta.Application.Validations.SaleValidations;

public class SetSaleWarrantyInputValidator : AbstractValidator<SetSaleWarrantyInput>
{
    public SetSaleWarrantyInputValidator()
    {
        RuleFor(x => x.Duration)
            .GreaterThan(0).WithMessage("O prazo da garantia deve ser maior que zero.");

        RuleFor(x => x.Unit)
            .IsInEnum().WithMessage("Unidade de garantia inválida.");
    }
}
