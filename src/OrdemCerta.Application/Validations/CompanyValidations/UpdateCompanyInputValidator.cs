using FluentValidation;
using OrdemCerta.Application.Inputs.CompanyInputs;

namespace OrdemCerta.Application.Validations.CompanyValidations;

public class UpdateCompanyInputValidator : AbstractValidator<UpdateCompanyInput>
{
    public UpdateCompanyInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome da empresa é obrigatório")
            .MaximumLength(200).WithMessage("O nome da empresa deve ter no máximo 200 caracteres");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("O telefone é obrigatório");
    }
}
