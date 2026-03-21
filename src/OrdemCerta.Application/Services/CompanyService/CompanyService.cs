using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using OrdemCerta.Application.Abstractions;
using OrdemCerta.Application.Inputs.CompanyInputs;
using OrdemCerta.Domain.Companies;
using OrdemCerta.Domain.Companies.DTOs;
using OrdemCerta.Domain.Companies.Extensions;
using OrdemCerta.Infrastructure.DataContext.Uow;
using OrdemCerta.Infrastructure.Repositories.CompanyRepository;
using OrdemCerta.Shared;
using System.Text.RegularExpressions;

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

        var (phone, areaCode, number) = ParsePhone(input.Phone);
        var cnpj = ParseCnpj(input.Cnpj);
        var email = input.Email.Trim().ToLower();
        var passwordHash = _passwordHasher.Hash(input.Password);

        var companyResult = Company.Create(
            input.Name,
            phone,
            areaCode,
            number,
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
        var (phone, areaCode, number) = ParsePhone(input.Phone);

        company.UpdateName(input.Name);
        company.UpdatePhone(phone, areaCode, number);
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

        var phone = "55" + company.Phone;
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

        var phone = "55" + company.Phone;
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

    private static (string phone, string areaCode, string number) ParsePhone(string input)
    {
        var digits = Regex.Replace(input, @"\D", "");
        var areaCode = digits[..2];
        var number = digits[2..];
        return (digits, areaCode, number);
    }

    private static string? ParseCnpj(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        return Regex.Replace(input, @"\D", "");
    }
}
