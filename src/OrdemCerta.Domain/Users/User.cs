using OrdemCerta.Domain.Users.ValueObjects;
using OrdemCerta.Shared;

namespace OrdemCerta.Domain.Users;

public class User : AggregateRoot
{
    public Guid CompanyId { get; private set; }
    public UserName Name { get; private set; } = null!;
    public UserEmail Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;

    protected User() { }

    private User(Guid companyId, UserName name, UserEmail email, string passwordHash)
    {
        Id = Guid.NewGuid();
        CompanyId = companyId;
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
    }

    public static Result<User> Create(Guid companyId, UserName name, UserEmail email, string passwordHash)
    {
        var user = new User(companyId, name, email, passwordHash);
        return user;
    }

    public Result UpdateName(UserName name)
    {
        Name = name;
        return Result.Success();
    }

    public Result UpdatePasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
        return Result.Success();
    }
}
