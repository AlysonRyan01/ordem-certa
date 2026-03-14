using OrdemCerta.Domain.Users.DTOs;

namespace OrdemCerta.Domain.Users.Extensions;

public static class UserExtensions
{
    public static UserOutput ToOutput(this User user)
    {
        return new UserOutput
        {
            Id = user.Id,
            CompanyId = user.CompanyId,
            Name = user.Name.Value,
            Email = user.Email.Value
        };
    }
}
