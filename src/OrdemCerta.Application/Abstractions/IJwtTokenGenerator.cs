using OrdemCerta.Domain.Users;

namespace OrdemCerta.Application.Abstractions;

public interface IJwtTokenGenerator
{
    (string token, DateTime expiresAt) Generate(User user);
}
