using FluentValidation;
using OrdemCerta.Application.Abstractions;
using OrdemCerta.Application.Inputs.UserInputs;
using OrdemCerta.Domain.Users.DTOs;
using OrdemCerta.Domain.Users.Extensions;
using OrdemCerta.Infrastructure.Repositories.UserRepository;
using OrdemCerta.Shared;

namespace OrdemCerta.Application.Services.AuthService;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IValidator<LoginInput> _loginValidator;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IValidator<LoginInput> loginValidator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _loginValidator = loginValidator;
    }

    public async Task<Result<TokenOutput>> LoginAsync(LoginInput input, CancellationToken cancellationToken)
    {
        var validationResult = await _loginValidator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
            return Result<TokenOutput>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var userResult = await _userRepository.GetByEmailAsync(input.Email.Trim().ToLower(), cancellationToken);
        if (userResult.IsFailure)
            return "E-mail ou senha inválidos";

        var user = userResult.Value!;

        if (!_passwordHasher.Verify(input.Password, user.PasswordHash))
            return "E-mail ou senha inválidos";

        var (token, expiresAt) = _jwtTokenGenerator.Generate(user);

        return new TokenOutput
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = user.ToOutput()
        };
    }
}
