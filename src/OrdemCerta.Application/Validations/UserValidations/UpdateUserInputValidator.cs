using FluentValidation;
using OrdemCerta.Application.Inputs.UserInputs;

namespace OrdemCerta.Application.Validations.UserValidations;

public class UpdateUserInputValidator : AbstractValidator<UpdateUserInput>
{
    public UpdateUserInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório")
            .MaximumLength(200).WithMessage("O nome deve ter no máximo 200 caracteres");
    }
}
