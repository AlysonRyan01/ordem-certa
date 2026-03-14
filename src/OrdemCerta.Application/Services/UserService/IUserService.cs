using OrdemCerta.Application.Inputs.UserInputs;
using OrdemCerta.Domain.Users.DTOs;
using OrdemCerta.Shared;

namespace OrdemCerta.Application.Services.UserService;

public interface IUserService
{
    Task<Result<UserOutput>> RegisterAsync(RegisterUserInput input, CancellationToken cancellationToken);
    Task<Result<UserOutput>> UpdateAsync(Guid id, UpdateUserInput input, CancellationToken cancellationToken);
    Task<Result<UserOutput>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
