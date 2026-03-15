using FluentValidation;
using OrdemCerta.Application.Abstractions;
using OrdemCerta.Application.Inputs.CompanyInputs;
using OrdemCerta.Domain.Companies.DTOs;
using OrdemCerta.Infrastructure.Repositories.CompanyRepository;
using OrdemCerta.Shared;

namespace OrdemCerta.Application.Services.AuthService;

public class AuthService : IAuthService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IValidator<LoginInput> _loginValidator;

    public AuthService(
        ICompanyRepository companyRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IValidator<LoginInput> loginValidator)
    {
        _companyRepository = companyRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _loginValidator = loginValidator;
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

        var (token, expiresAt) = _jwtTokenGenerator.Generate(company);

        return new TokenOutput
        {
            Token = token,
            ExpiresAt = expiresAt,
        };
    }
}
