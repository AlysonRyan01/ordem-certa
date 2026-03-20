namespace OrdemCerta.Domain.Admin;

public class AdminUser
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    protected AdminUser() { }

    private AdminUser(string email, string passwordHash)
    {
        Id = Guid.NewGuid();
        Email = email;
        PasswordHash = passwordHash;
        CreatedAt = DateTime.UtcNow;
    }

    public static AdminUser Create(string email, string passwordHash)
        => new(email.Trim().ToLower(), passwordHash);
}
