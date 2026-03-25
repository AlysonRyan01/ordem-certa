using FluentValidation;
using OrdemCerta.Application.Abstractions;
using OrdemCerta.Application.Inputs.CompanyInputs;
using OrdemCerta.Domain.Companies.DTOs;
using OrdemCerta.Infrastructure.DataContext.Uow;
using OrdemCerta.Infrastructure.Repositories.CompanyRepository;
using OrdemCerta.Shared;
using System.Security.Cryptography;

namespace OrdemCerta.Application.Services.AuthService;

public class AuthService : IAuthService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IValidator<LoginInput> _loginValidator;
    private readonly IUnitOfWork _unitOfWork;

    private const int RefreshTokenExpirationDays = 30;

    public AuthService(
        ICompanyRepository companyRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IValidator<LoginInput> loginValidator,
        IUnitOfWork unitOfWork)
    {
        _companyRepository = companyRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _loginValidator = loginValidator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TokenOutput>> LoginAsync(LoginInput input, CancellationToken cancellationToken)
    {
        var validationResult = await _loginValidator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
            return Result<TokenOutput>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var companyResult = await _companyRepository.GetByEmailAsync(input.Email.Trim().ToLower(), cancellationToken);
        if (companyResult.IsFailure)
            return "E-mail ou senha inválidos";

        var company = companyResult.Value!;

        if (!_passwordHasher.Verify(input.Password, company.PasswordHash))
            return "E-mail ou senha inválidos";

        var (accessToken, expiresAt) = _jwtTokenGenerator.Generate(company);
        var (refreshToken, refreshExpiresAt) = GenerateRefreshToken();

        company.SetRefreshToken(refreshToken, refreshExpiresAt);
        await _companyRepository.UpdateAsync(company, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return new TokenOutput
        {
            Token = accessToken,
            ExpiresAt = expiresAt,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshExpiresAt,
        };
    }

    public async Task<Result<TokenOutput>> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var companyResult = await _companyRepository.GetByRefreshTokenAsync(refreshToken, cancellationToken);
        if (companyResult.IsFailure)
            return "Refresh token inválido";

        var company = companyResult.Value!;

        if (company.RefreshTokenExpiresAt < DateTime.UtcNow)
            return "Refresh token expirado";

        var (accessToken, expiresAt) = _jwtTokenGenerator.Generate(company);
        var (newRefreshToken, refreshExpiresAt) = GenerateRefreshToken();

        company.SetRefreshToken(newRefreshToken, refreshExpiresAt);
        await _companyRepository.UpdateAsync(company, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return new TokenOutput
        {
            Token = accessToken,
            ExpiresAt = expiresAt,
            RefreshToken = newRefreshToken,
            RefreshTokenExpiresAt = refreshExpiresAt,
        };
    }

    private static (string token, DateTime expiresAt) GenerateRefreshToken()
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return (token, DateTime.UtcNow.AddDays(RefreshTokenExpirationDays));
    }
}
