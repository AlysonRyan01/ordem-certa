using Microsoft.AspNetCore.Http;
using OrdemCerta.Domain.Companies.Interfaces;

namespace OrdemCerta.Application.Security;

public class CurrentCompany : ICurrentCompany
{
    public Guid CompanyId { get; }

    public CurrentCompany(IHttpContextAccessor accessor)
    {
        var claim = accessor.HttpContext?.User.FindFirst("companyId")?.Value;
        CompanyId = Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}
