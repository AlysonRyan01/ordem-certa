using FluentValidation;
using OrdemCerta.Application.Inputs.ServiceOrderInputs;

namespace OrdemCerta.Application.Validations.ServiceOrderValidations;

public class CreateServiceOrderInputValidator : AbstractValidator<CreateServiceOrderInput>
{
    public CreateServiceOrderInputValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("O cliente é obrigatório");

        RuleFor(x => x.DeviceType)
            .NotEmpty().WithMessage("O tipo de aparelho é obrigatório")
            .MaximumLength(100).WithMessage("O tipo de aparelho deve ter no máximo 100 caracteres");

        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("A marca é obrigatória")
            .MaximumLength(100).WithMessage("A marca deve ter no máximo 100 caracteres");

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("O modelo é obrigatório")
            .MaximumLength(100).WithMessage("O modelo deve ter no máximo 100 caracteres");

        RuleFor(x => x.ReportedDefect)
            .NotEmpty().WithMessage("O defeito relatado é obrigatório")
            .MaximumLength(500).WithMessage("O defeito relatado deve ter no máximo 500 caracteres");
    }
}
