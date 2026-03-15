namespace OrdemCerta.Domain.Companies.DTOs;

public class TokenOutput
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
