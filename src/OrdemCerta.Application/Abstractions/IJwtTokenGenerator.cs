using OrdemCerta.Domain.Admin;
using OrdemCerta.Domain.Companies;

namespace OrdemCerta.Application.Abstractions;

public interface IJwtTokenGenerator
{
    (string token, DateTime expiresAt) Generate(Company company);
    (string token, DateTime expiresAt) GenerateForAdmin(AdminUser admin);
}
