namespace OrdemCerta.Domain.Users.DTOs;

public class TokenOutput
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserOutput User { get; set; } = null!;
}
