using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OrdemCerta.Application.Abstractions;
using OrdemCerta.Domain.Companies;

namespace OrdemCerta.Application.Security;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationHours;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _secretKey = configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey não configurado");
        _issuer = configuration["Jwt:Issuer"] ?? "OrdemCerta";
        _audience = configuration["Jwt:Audience"] ?? "OrdemCerta";
        _expirationHours = int.TryParse(configuration["Jwt:ExpirationHours"], out var h) ? h : 24;
    }

    public (string token, DateTime expiresAt) Generate(Company company)
    {
        var expiresAt = DateTime.UtcNow.AddHours(_expirationHours);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, company.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, company.Email),
            new Claim(JwtRegisteredClaimNames.Name, company.Name.Value),
            new Claim("companyId", company.Id.ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        return (token, expiresAt);
    }
}
