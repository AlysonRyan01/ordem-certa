using FluentValidation;
using OrdemCerta.Application.Inputs.ServiceOrderInputs;
using OrdemCerta.Domain.ServiceOrders.Enums;

namespace OrdemCerta.Application.Validations.ServiceOrderValidations;

public class ChangeStatusInputValidator : AbstractValidator<ChangeStatusInput>
{
    public ChangeStatusInputValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status inválido");
    }
}
