using OrdemCerta.Domain.Companies;

namespace OrdemCerta.Application.Abstractions;

public interface IJwtTokenGenerator
{
    (string token, DateTime expiresAt) Generate(Company company);
}
