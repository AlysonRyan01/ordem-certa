using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using OrdemCerta.Application.Abstractions;
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
    private readonly IPasswordHasher _passwordHasher;
    private readonly IWhatsAppService _whatsAppService;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    private readonly IValidator<CreateCompanyInput> _createValidator;
    private readonly IValidator<UpdateCompanyInput> _updateValidator;

    public CompanyService(
        ICompanyRepository companyRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IWhatsAppService whatsAppService,
        IMemoryCache cache,
        IConfiguration configuration,
        IValidator<CreateCompanyInput> createValidator,
        IValidator<UpdateCompanyInput> updateValidator)
    {
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _whatsAppService = whatsAppService;
        _cache = cache;
        _configuration = configuration;
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

        var email = input.Email.Trim().ToLower();
        var passwordHash = _passwordHasher.Hash(input.Password);

        var companyResult = Company.Create(
            nameResult.Value!,
            phoneResult.Value!,
            email,
            passwordHash,
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

    public async Task<Result> RequestPasswordChangeAsync(Guid companyId, CancellationToken cancellationToken)
    {
        var companyResult = await _companyRepository.GetByIdAsync(companyId, cancellationToken);
        if (companyResult.IsFailure)
            return Result.Failure(companyResult.Errors);

        var company = companyResult.Value!;

        var code = Random.Shared.Next(100000, 999999).ToString();
        var cacheKey = $"pwd_code_{companyId}";
        _cache.Set(cacheKey, code, TimeSpan.FromMinutes(5));

        var phone = "55" + company.Phone.Value;
        var instance = _configuration["EvolutionApi:Instance"] ?? "";
        var message = $"*OrdemCerta*\n\nSeu código para alterar a senha é: *{code}*\n\nEste código expira em 5 minutos.";

        await _whatsAppService.SendTextAsync(phone, message, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ConfirmPasswordChangeAsync(Guid companyId, ConfirmPasswordChangeInput input, CancellationToken cancellationToken)
    {
        var cacheKey = $"pwd_code_{companyId}";
        if (!_cache.TryGetValue(cacheKey, out string? storedCode) || storedCode != input.Code)
            return Result.Failure("Código inválido ou expirado.");

        var companyResult = await _companyRepository.GetByIdAsync(companyId, cancellationToken);
        if (companyResult.IsFailure)
            return Result.Failure(companyResult.Errors);

        var company = companyResult.Value!;
        var newHash = _passwordHasher.Hash(input.NewPassword);
        company.UpdatePasswordHash(newHash);

        await _companyRepository.UpdateAsync(company, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        _cache.Remove(cacheKey);

        return Result.Success();
    }

    public async Task<Result> RequestPasswordResetAsync(RequestPasswordResetInput input, CancellationToken cancellationToken)
    {
        var email = input.Email.Trim().ToLower();
        var companyResult = await _companyRepository.GetByEmailAsync(email, cancellationToken);
        if (companyResult.IsFailure)
            return Result.Success(); // Não revelar se o e-mail existe

        var company = companyResult.Value!;
        var code = Random.Shared.Next(100000, 999999).ToString();
        var cacheKey = $"pwd_reset_{email}";
        _cache.Set(cacheKey, code, TimeSpan.FromMinutes(5));

        var phone = "55" + company.Phone.Value;
        var instance = _configuration["EvolutionApi:Instance"] ?? "";
        var message = $"*OrdemCerta*\n\nSeu código para redefinir a senha é: *{code}*\n\nEste código expira em 5 minutos.";

        await _whatsAppService.SendTextAsync(phone, message, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ConfirmPasswordResetAsync(ConfirmPasswordResetInput input, CancellationToken cancellationToken)
    {
        var email = input.Email.Trim().ToLower();
        var cacheKey = $"pwd_reset_{email}";

        if (!_cache.TryGetValue(cacheKey, out string? storedCode) || storedCode != input.Code)
            return Result.Failure("Código inválido ou expirado.");

        var companyResult = await _companyRepository.GetByEmailAsync(email, cancellationToken);
        if (companyResult.IsFailure)
            return Result.Failure(companyResult.Errors);

        var company = companyResult.Value!;
        var newHash = _passwordHasher.Hash(input.NewPassword);
        company.UpdatePasswordHash(newHash);

        await _companyRepository.UpdateAsync(company, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        _cache.Remove(cacheKey);

        return Result.Success();
    }
}
