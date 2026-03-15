using FluentValidation;
using OrdemCerta.Application.Inputs.ServiceOrderInputs;

namespace OrdemCerta.Application.Validations.ServiceOrderValidations;

public class CreateBudgetInputValidator : AbstractValidator<CreateBudgetInput>
{
    public CreateBudgetInputValidator()
    {
        RuleFor(x => x.Value)
            .GreaterThan(0).WithMessage("O valor do orçamento deve ser maior que zero");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("A descrição do orçamento é obrigatória")
            .MaximumLength(500).WithMessage("A descrição deve ter no máximo 500 caracteres");
    }
}
