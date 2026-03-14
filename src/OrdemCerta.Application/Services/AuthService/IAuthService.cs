using OrdemCerta.Application.Inputs.UserInputs;
using OrdemCerta.Domain.Users.DTOs;
using OrdemCerta.Shared;

namespace OrdemCerta.Application.Services.AuthService;

public interface IAuthService
{
    Task<Result<TokenOutput>> LoginAsync(LoginInput input, CancellationToken cancellationToken);
}
