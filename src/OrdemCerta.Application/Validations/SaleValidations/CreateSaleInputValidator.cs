using FluentValidation;
using OrdemCerta.Application.Inputs.SaleInputs;

namespace OrdemCerta.Application.Validations.SaleValidations;

public class CreateSaleInputValidator : AbstractValidator<CreateSaleInput>
{
    public CreateSaleInputValidator()
    {
        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Forma de pagamento inválida.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("A descrição deve ter no máximo 500 caracteres.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("As observações devem ter no máximo 1000 caracteres.");

        RuleFor(x => x.CustomerName)
            .MaximumLength(200).WithMessage("O nome do cliente deve ter no máximo 200 caracteres.");
    }
}
