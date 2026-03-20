using OrdemCerta.Application.Inputs.SaleInputs;
using OrdemCerta.Domain.Sales.DTOs;
using OrdemCerta.Domain.Sales.Enums;
using OrdemCerta.Shared;

namespace OrdemCerta.Application.Services.SaleService;

public interface ISaleService
{
    Task<Result<SaleOutput>> CreateAsync(CreateSaleInput input, CancellationToken cancellationToken);
    Task<Result<SaleOutput>> UpdateAsync(Guid id, UpdateSaleInput input, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<SaleOutput>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<List<SaleOutput>>> GetPagedAsync(GetPagedInput input, CancellationToken cancellationToken);
    Task<Result<List<SaleOutput>>> GetByStatusAsync(SaleStatus status, GetPagedInput input, CancellationToken cancellationToken);
    Task<Result<List<SaleOutput>>> GetByCustomerAsync(Guid customerId, GetPagedInput input, CancellationToken cancellationToken);
    Task<Result<SaleOutput>> AddItemAsync(Guid id, AddSaleItemInput input, CancellationToken cancellationToken);
    Task<Result<SaleOutput>> UpdateItemAsync(Guid id, Guid itemId, UpdateSaleItemInput input, CancellationToken cancellationToken);
    Task<Result<SaleOutput>> RemoveItemAsync(Guid id, Guid itemId, CancellationToken cancellationToken);
    Task<Result<SaleOutput>> CompleteAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<SaleOutput>> CancelAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<SaleOutput>> SetWarrantyAsync(Guid id, SetSaleWarrantyInput input, CancellationToken cancellationToken);
    Task<Result<byte[]>> PrintReceiptAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<byte[]>> PrintWarrantyAsync(Guid id, CancellationToken cancellationToken);
}
