using OrdemCerta.Application.Abstractions;
using OrdemCerta.Domain.Admin.DTOs;
using OrdemCerta.Infrastructure.Repositories.AdminRepository;
using OrdemCerta.Shared;

namespace OrdemCerta.Application.Services.AdminService;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _adminRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AdminService(
        IAdminRepository adminRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _adminRepository = adminRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<AdminTokenOutput>> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var admin = await _adminRepository.GetByEmailAsync(email.Trim().ToLower(), cancellationToken);

        if (admin is null || !_passwordHasher.Verify(password, admin.PasswordHash))
            return "E-mail ou senha inválidos";

        var (token, expiresAt) = _jwtTokenGenerator.GenerateForAdmin(admin);

        return new AdminTokenOutput(token, expiresAt);
    }

    public async Task<Result<AdminStatsOutput>> GetStatsAsync(CancellationToken cancellationToken)
    {
        return await _adminRepository.GetStatsAsync(cancellationToken);
    }

    public async Task<Result<List<AdminCompanyOutput>>> GetCompaniesAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        return await _adminRepository.GetCompaniesPagedAsync(page, pageSize, cancellationToken);
    }
}
