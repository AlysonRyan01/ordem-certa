namespace OrdemCerta.Domain.Users.DTOs;

public class UserOutput
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
