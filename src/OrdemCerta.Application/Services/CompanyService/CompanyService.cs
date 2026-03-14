using FluentValidation;
using OrdemCerta.Application.Inputs.CompanyInputs;
using OrdemCerta.Domain.Companies;
using OrdemCerta.Domain.Companies.DTOs;
using OrdemCerta.Domain.Companies.Extensions;
using OrdemCerta.Domain.Companies.ValueObjects;
using OrdemCerta.Infrastructure.DataContext.Uow;
using OrdemCerta.Infrastructure.Repositories.CompanyRepository;
using OrdemCerta.Shared;

namespace OrdemCerta.Application.Services.CompanyService;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateCompanyInput> _createValidator;
    private readonly IValidator<UpdateCompanyInput> _updateValidator;

    public CompanyService(
        ICompanyRepository companyRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateCompanyInput> createValidator,
        IValidator<UpdateCompanyInput> updateValidator)
    {
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<Result<CompanyOutput>> CreateAsync(CreateCompanyInput input, CancellationToken cancellationToken)
    {
        var validationResult = await _createValidator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
            return Result<CompanyOutput>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var nameResult = CompanyName.Create(input.Name);
        if (nameResult.IsFailure)
            return Result<CompanyOutput>.Failure(nameResult.Errors);

        CompanyCnpj? cnpj = null;
        if (!string.IsNullOrWhiteSpace(input.Cnpj))
        {
            var cnpjResult = CompanyCnpj.Create(input.Cnpj);
            if (cnpjResult.IsFailure)
                return Result<CompanyOutput>.Failure(cnpjResult.Errors);

            cnpj = cnpjResult.Value;
        }

        var phoneResult = CompanyPhone.Create(input.Phone);
        if (phoneResult.IsFailure)
            return Result<CompanyOutput>.Failure(phoneResult.Errors);

        var companyResult = Company.Create(
            nameResult.Value!,
            phoneResult.Value!,
            cnpj,
            input.Street,
            input.Number,
            input.City,
            input.State);

        if (companyResult.IsFailure)
            return Result<CompanyOutput>.Failure(companyResult.Errors);

        await _companyRepository.AddAsync(companyResult.Value!, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return companyResult.Value!.ToOutput();
    }

    public async Task<Result<CompanyOutput>> UpdateAsync(Guid id, UpdateCompanyInput input, CancellationToken cancellationToken)
    {
        var validationResult = await _updateValidator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
            return Result<CompanyOutput>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var companyResult = await _companyRepository.GetByIdAsync(id, cancellationToken);
        if (companyResult.IsFailure)
            return Result<CompanyOutput>.Failure(companyResult.Errors);

        var company = companyResult.Value!;

        var nameResult = CompanyName.Create(input.Name);
        if (nameResult.IsFailure)
            return Result<CompanyOutput>.Failure(nameResult.Errors);

        var phoneResult = CompanyPhone.Create(input.Phone);
        if (phoneResult.IsFailure)
            return Result<CompanyOutput>.Failure(phoneResult.Errors);

        company.UpdateName(nameResult.Value!);
        company.UpdatePhone(phoneResult.Value!);
        company.UpdateAddress(input.Street, input.Number, input.City, input.State);

        await _companyRepository.UpdateAsync(company, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return company.ToOutput();
    }

    public async Task<Result<CompanyOutput>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var companyResult = await _companyRepository.GetByIdAsync(id, cancellationToken);
        if (companyResult.IsFailure)
            return Result<CompanyOutput>.Failure(companyResult.Errors);

        return companyResult.Value!.ToOutput();
    }
}
