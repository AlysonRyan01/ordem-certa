using OrdemCerta.Application.Inputs.CompanyInputs;
using OrdemCerta.Domain.Companies.DTOs;
using OrdemCerta.Shared;

namespace OrdemCerta.Application.Services.AuthService;

public interface IAuthService
{
    Task<Result<TokenOutput>> LoginAsync(LoginInput input, CancellationToken cancellationToken);
    Task<Result<TokenOutput>> RefreshAsync(string refreshToken, CancellationToken cancellationToken);
}
