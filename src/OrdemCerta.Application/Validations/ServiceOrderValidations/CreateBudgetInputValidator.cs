using FluentValidation;
using OrdemCerta.Application.Inputs.ServiceOrderInputs;

namespace OrdemCerta.Application.Validations.ServiceOrderValidations;

public class CreateBudgetInputValidator : AbstractValidator<CreateBudgetInput>
{
    public CreateBudgetInputValidator()
    {
        RuleFor(x => x.Value)
            .GreaterThanOrEqualTo(0).WithMessage("O valor do orçamento não pode ser negativo");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("A descrição do orçamento é obrigatória")
            .MaximumLength(500).WithMessage("A descrição deve ter no máximo 500 caracteres");

        RuleFor(x => x.RepairResult)
            .IsInEnum().WithMessage("Resultado do diagnóstico inválido.");

        RuleFor(x => x.WarrantyDuration)
            .GreaterThan(0).When(x => x.WarrantyDuration.HasValue)
            .WithMessage("O prazo da garantia deve ser maior que zero.");

        RuleFor(x => x.WarrantyUnit)
            .NotNull().When(x => x.WarrantyDuration.HasValue)
            .WithMessage("Informe a unidade da garantia.");

        RuleFor(x => x.WarrantyDuration)
            .NotNull().When(x => x.WarrantyUnit.HasValue)
            .WithMessage("Informe o prazo da garantia.");
    }
}
