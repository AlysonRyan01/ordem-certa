using OrdemCerta.Application.Inputs.CustomerInputs;
using OrdemCerta.Domain.Customers.DTOs;
using OrdemCerta.Shared;

namespace OrdemCerta.Application.Services.CustomerService;

public interface ICustomerService
{
    Task<Result<CustomerOutput>> CreateAsync(CreateCustomerInput input, CancellationToken cancellationToken = default);
    Task<Result<CustomerOutput>> UpdateAsync(Guid id, UpdateCustomerInput input, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<CustomerOutput>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<List<CustomerOutput>>> GetPagedAsync(GetPagedInput input, CancellationToken cancellationToken = default);
    Task<Result<List<CustomerOutput>>> GetByNameAsync(string searchTerm, GetPagedInput input, CancellationToken cancellationToken = default);
    Task<Result> AddPhoneAsync(Guid id, AddPhoneInput input, CancellationToken cancellationToken = default);
    Task<Result> RemovePhoneAsync(Guid id, RemovePhoneInput input, CancellationToken cancellationToken = default);
}