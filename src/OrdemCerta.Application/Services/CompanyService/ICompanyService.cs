using OrdemCerta.Application.Inputs.CompanyInputs;
using OrdemCerta.Domain.Companies.DTOs;
using OrdemCerta.Shared;

namespace OrdemCerta.Application.Services.CompanyService;

public interface ICompanyService
{
    Task<Result<CompanyOutput>> CreateAsync(CreateCompanyInput input, CancellationToken cancellationToken);
    Task<Result<CompanyOutput>> UpdateAsync(Guid id, UpdateCompanyInput input, CancellationToken cancellationToken);
    Task<Result<CompanyOutput>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Result> RequestPasswordChangeAsync(Guid companyId, CancellationToken cancellationToken);
    Task<Result> ConfirmPasswordChangeAsync(Guid companyId, ConfirmPasswordChangeInput input, CancellationToken cancellationToken);
}
