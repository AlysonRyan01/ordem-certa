using FluentValidation;
using QuestPDF.Infrastructure;
using OrdemCerta.Application.Inputs.SaleInputs;
using OrdemCerta.Application.Services.PdfService;
using OrdemCerta.Domain.Companies.Extensions;
using OrdemCerta.Domain.Companies.Interfaces;
using OrdemCerta.Domain.Sales;
using OrdemCerta.Domain.Sales.DTOs;
using OrdemCerta.Domain.Sales.Enums;
using OrdemCerta.Domain.Sales.Extensions;
using OrdemCerta.Infrastructure.DataContext.Uow;
using OrdemCerta.Infrastructure.Repositories.CompanyRepository;
using OrdemCerta.Infrastructure.Repositories.CustomerRepository;
using OrdemCerta.Infrastructure.Repositories.SaleRepository;
using OrdemCerta.Shared;

namespace OrdemCerta.Application.Services.SaleService;

public class SaleService : ISaleService
{
    private readonly ISaleRepository _saleRepository;
    private readonly ICompanySaleSequenceRepository _sequenceRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IPdfService _pdfService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentCompany _currentCompany;
    private readonly IValidator<CreateSaleInput> _createValidator;
    private readonly IValidator<UpdateSaleInput> _updateValidator;
    private readonly IValidator<SetSaleWarrantyInput> _warrantyValidator;

    public SaleService(
        ISaleRepository saleRepository,
        ICompanySaleSequenceRepository sequenceRepository,
        ICompanyRepository companyRepository,
        ICustomerRepository customerRepository,
        IPdfService pdfService,
        IUnitOfWork unitOfWork,
        ICurrentCompany currentCompany,
        IValidator<CreateSaleInput> createValidator,
        IValidator<UpdateSaleInput> updateValidator,
        IValidator<SetSaleWarrantyInput> warrantyValidator)
    {
        _saleRepository = saleRepository;
        _sequenceRepository = sequenceRepository;
        _companyRepository = companyRepository;
        _customerRepository = customerRepository;
        _pdfService = pdfService;
        _unitOfWork = unitOfWork;
        _currentCompany = currentCompany;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _warrantyValidator = warrantyValidator;
    }

    public async Task<Result<SaleOutput>> CreateAsync(CreateSaleInput input, CancellationToken cancellationToken)
    {
        var validationResult = await _createValidator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
            return Result<SaleOutput>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var saleNumber = await _sequenceRepository.GetNextNumberAsync(_currentCompany.CompanyId, cancellationToken);

        var saleResult = Sale.Create(
            _currentCompany.CompanyId,
            input.CustomerId,
            saleNumber,
            input.CustomerName,
            input.Description,
            input.PaymentMethod,
            input.Notes);

        if (saleResult.IsFailure)
            return Result<SaleOutput>.Failure(saleResult.Errors);

        var sale = saleResult.Value!;

        foreach (var item in input.Items)
        {
            var addResult = sale.AddItem(item.Description, item.Quantity, item.UnitPrice);
            if (addResult.IsFailure)
                return Result<SaleOutput>.Failure(addResult.Errors);
        }

        await _saleRepository.AddAsync(sale, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return sale.ToOutput();
    }

    public async Task<Result<SaleOutput>> UpdateAsync(Guid id, UpdateSaleInput input, CancellationToken cancellationToken)
    {
        var validationResult = await _updateValidator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
            return Result<SaleOutput>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var saleResult = await _saleRepository.GetByIdTrackedAsync(id, cancellationToken);
        if (saleResult.IsFailure)
            return Result<SaleOutput>.Failure(saleResult.Errors);

        var sale = saleResult.Value!;
        var updateResult = sale.Update(input.CustomerId, input.CustomerName, input.Description, input.PaymentMethod, input.Notes);
        if (updateResult.IsFailure)
            return Result<SaleOutput>.Failure(updateResult.Errors);

        await _saleRepository.UpdateAsync(sale, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return sale.ToOutput();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var saleResult = await _saleRepository.GetByIdAsync(id, cancellationToken);
        if (saleResult.IsFailure)
            return Result.Failure(saleResult.Errors);

        await _saleRepository.DeleteAsync(saleResult.Value!, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<SaleOutput>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var saleResult = await _saleRepository.GetByIdAsync(id, cancellationToken);
        if (saleResult.IsFailure)
            return Result<SaleOutput>.Failure(saleResult.Errors);

        return saleResult.Value!.ToOutput();
    }

    public async Task<Result<List<SaleOutput>>> GetPagedAsync(GetPagedInput input, CancellationToken cancellationToken)
    {
        var sales = await _saleRepository.GetPagedAsync(input.Page, input.PageSize, cancellationToken);
        return sales.ToOutputList();
    }

    public async Task<Result<List<SaleOutput>>> GetByStatusAsync(SaleStatus status, GetPagedInput input, CancellationToken cancellationToken)
    {
        var sales = await _saleRepository.GetByStatusAsync(status, input.Page, input.PageSize, cancellationToken);
        return sales.ToOutputList();
    }

    public async Task<Result<List<SaleOutput>>> GetByCustomerAsync(Guid customerId, GetPagedInput input, CancellationToken cancellationToken)
    {
        var sales = await _saleRepository.GetByCustomerAsync(customerId, input.Page, input.PageSize, cancellationToken);
        return sales.ToOutputList();
    }

    public async Task<Result<SaleOutput>> AddItemAsync(Guid id, AddSaleItemInput input, CancellationToken cancellationToken)
    {
        var saleResult = await _saleRepository.GetByIdTrackedAsync(id, cancellationToken);
        if (saleResult.IsFailure)
            return Result<SaleOutput>.Failure(saleResult.Errors);

        var sale = saleResult.Value!;
        var addResult = sale.AddItem(input.Description, input.Quantity, input.UnitPrice);
        if (addResult.IsFailure)
            return Result<SaleOutput>.Failure(addResult.Errors);

        await _saleRepository.UpdateAsync(sale, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return sale.ToOutput();
    }

    public async Task<Result<SaleOutput>> UpdateItemAsync(Guid id, Guid itemId, UpdateSaleItemInput input, CancellationToken cancellationToken)
    {
        var saleResult = await _saleRepository.GetByIdTrackedAsync(id, cancellationToken);
        if (saleResult.IsFailure)
            return Result<SaleOutput>.Failure(saleResult.Errors);

        var sale = saleResult.Value!;
        var updateResult = sale.UpdateItem(itemId, input.Description, input.Quantity, input.UnitPrice);
        if (updateResult.IsFailure)
            return Result<SaleOutput>.Failure(updateResult.Errors);

        await _saleRepository.UpdateAsync(sale, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return sale.ToOutput();
    }

    public async Task<Result<SaleOutput>> RemoveItemAsync(Guid id, Guid itemId, CancellationToken cancellationToken)
    {
        var saleResult = await _saleRepository.GetByIdTrackedAsync(id, cancellationToken);
        if (saleResult.IsFailure)
            return Result<SaleOutput>.Failure(saleResult.Errors);

        var sale = saleResult.Value!;
        var removeResult = sale.RemoveItem(itemId);
        if (removeResult.IsFailure)
            return Result<SaleOutput>.Failure(removeResult.Errors);

        await _saleRepository.UpdateAsync(sale, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return sale.ToOutput();
    }

    public async Task<Result<SaleOutput>> CompleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var saleResult = await _saleRepository.GetByIdTrackedAsync(id, cancellationToken);
        if (saleResult.IsFailure)
            return Result<SaleOutput>.Failure(saleResult.Errors);

        var sale = saleResult.Value!;
        var completeResult = sale.Complete();
        if (completeResult.IsFailure)
            return Result<SaleOutput>.Failure(completeResult.Errors);

        await _saleRepository.UpdateAsync(sale, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return sale.ToOutput();
    }

    public async Task<Result<SaleOutput>> CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        var saleResult = await _saleRepository.GetByIdTrackedAsync(id, cancellationToken);
        if (saleResult.IsFailure)
            return Result<SaleOutput>.Failure(saleResult.Errors);

        var sale = saleResult.Value!;
        var cancelResult = sale.Cancel();
        if (cancelResult.IsFailure)
            return Result<SaleOutput>.Failure(cancelResult.Errors);

        await _saleRepository.UpdateAsync(sale, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return sale.ToOutput();
    }

    public async Task<Result<SaleOutput>> SetWarrantyAsync(Guid id, SetSaleWarrantyInput input, CancellationToken cancellationToken)
    {
        var validationResult = await _warrantyValidator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
            return Result<SaleOutput>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var saleResult = await _saleRepository.GetByIdTrackedAsync(id, cancellationToken);
        if (saleResult.IsFailure)
            return Result<SaleOutput>.Failure(saleResult.Errors);

        var sale = saleResult.Value!;
        var setResult = sale.SetWarranty(input.Duration, input.Unit);
        if (setResult.IsFailure)
            return Result<SaleOutput>.Failure(setResult.Errors);

        await _saleRepository.UpdateAsync(sale, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return sale.ToOutput();
    }

    public async Task<Result<byte[]>> PrintReceiptAsync(Guid id, CancellationToken cancellationToken)
    {
        var saleResult = await _saleRepository.GetByIdAsync(id, cancellationToken);
        if (saleResult.IsFailure)
            return Result<byte[]>.Failure(saleResult.Errors);

        var sale = saleResult.Value!;

        var companyResult = await _companyRepository.GetByIdAsync(sale.CompanyId, cancellationToken);
        if (companyResult.IsFailure)
            return Result<byte[]>.Failure(companyResult.Errors);

        var saleOutput = sale.ToOutput();
        var companyOutput = companyResult.Value!.ToOutput();

        string? customerName = sale.CustomerName;
        string? customerPhone = null;
        string? customerDocument = null;

        if (sale.CustomerId.HasValue)
        {
            var customerResult = await _customerRepository.GetByIdAsync(sale.CustomerId.Value, cancellationToken);
            if (customerResult.IsSuccess)
            {
                var customer = customerResult.Value!;
                customerName = customer.FullName;
                customerPhone = customer.GetPhoneFormatted();
                customerDocument = customer.GetDocumentFormatted();
            }
        }

        QuestPDF.Settings.License = LicenseType.Community;

        var pdf = _pdfService.GenerateSaleReceipt(saleOutput, companyOutput, customerName, customerPhone, customerDocument);
        return pdf;
    }

    public async Task<Result<byte[]>> PrintWarrantyAsync(Guid id, CancellationToken cancellationToken)
    {
        var saleResult = await _saleRepository.GetByIdAsync(id, cancellationToken);
        if (saleResult.IsFailure)
            return Result<byte[]>.Failure(saleResult.Errors);

        var sale = saleResult.Value!;

        if (sale.WarrantyDuration is null)
            return "Esta venda não possui garantia registrada.";

        var companyResult = await _companyRepository.GetByIdAsync(sale.CompanyId, cancellationToken);
        if (companyResult.IsFailure)
            return Result<byte[]>.Failure(companyResult.Errors);

        var saleOutput = sale.ToOutput();
        var companyOutput = companyResult.Value!.ToOutput();

        string? customerName = sale.CustomerName;
        string? customerPhone = null;
        string? customerDocument = null;

        if (sale.CustomerId.HasValue)
        {
            var customerResult = await _customerRepository.GetByIdAsync(sale.CustomerId.Value, cancellationToken);
            if (customerResult.IsSuccess)
            {
                var customer = customerResult.Value!;
                customerName = customer.FullName;
                customerPhone = customer.GetPhoneFormatted();
                customerDocument = customer.GetDocumentFormatted();
            }
        }

        QuestPDF.Settings.License = LicenseType.Community;

        var pdf = _pdfService.GenerateSaleWarranty(saleOutput, companyOutput, customerName, customerPhone, customerDocument);
        return pdf;
    }
}
