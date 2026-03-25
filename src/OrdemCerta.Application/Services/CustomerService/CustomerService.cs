using FluentValidation;
using OrdemCerta.Application.Inputs.CustomerInputs;
using OrdemCerta.Domain.Companies.Interfaces;
using OrdemCerta.Domain.Customers;
using OrdemCerta.Domain.Customers.DTOs;
using OrdemCerta.Domain.Customers.Enums;
using OrdemCerta.Domain.Customers.Extensions;
using OrdemCerta.Infrastructure.DataContext.Uow;
using OrdemCerta.Infrastructure.Repositories.CustomerRepository;
using OrdemCerta.Shared;
using System.Text.RegularExpressions;

namespace OrdemCerta.Application.Services.CustomerService;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentCompany _currentCompany;
    private readonly IValidator<CreateCustomerInput> _createValidator;
    private readonly IValidator<UpdateCustomerInput> _updateValidator;

    public CustomerService(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        ICurrentCompany currentCompany,
        IValidator<CreateCustomerInput> createValidator,
        IValidator<UpdateCustomerInput> updateValidator)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _currentCompany = currentCompany;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<Result<CustomerOutput>> CreateAsync(CreateCustomerInput input, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
            return Result<CustomerOutput>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var (phone, areaCode, number) = ParsePhone(input.Phone);
        if (phone is null)
            return Result<CustomerOutput>.Failure("Telefone inválido");

        (string? document, CustomerDocumentType? documentType) = ParseDocument(input.Document);

        var customerResult = Customer.Create(
            _currentCompany.CompanyId,
            input.FullName,
            phone,
            areaCode!,
            number!,
            input.Email,
            input.Street,
            input.Number,
            input.City,
            input.State,
            document,
            documentType
        );

        if (customerResult.IsFailure)
            return Result<CustomerOutput>.Failure(customerResult.Errors);

        await _customerRepository.AddAsync(customerResult.Value!, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return customerResult.Value!.ToOutput();
    }

    public async Task<Result<CustomerOutput>> UpdateAsync(Guid id, UpdateCustomerInput input, CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
            return Result<CustomerOutput>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var customerResult = await _customerRepository.GetByIdTrackedAsync(id, cancellationToken);
        if (customerResult.IsFailure)
            return Result<CustomerOutput>.Failure(customerResult.Errors);

        var customer = customerResult.Value!;

        customer.UpdateName(input.FullName);

        var (phone, areaCode, number) = ParsePhone(input.Phone);
        if (phone is null)
            return Result<CustomerOutput>.Failure("Telefone inválido");

        customer.UpdatePhone(phone, areaCode!, number!);

        if (!string.IsNullOrWhiteSpace(input.Email))
            customer.UpdateEmail(input.Email);

        if (!string.IsNullOrWhiteSpace(input.Street) || !string.IsNullOrWhiteSpace(input.City))
            customer.UpdateAddress(input.Street, input.Number, input.City, input.State);

        await _customerRepository.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return customer.ToOutput();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customerResult = await _customerRepository.GetByIdTrackedAsync(id, cancellationToken);
        if (customerResult.IsFailure)
            return Result.Failure(customerResult.Errors);

        await _customerRepository.DeleteAsync(customerResult.Value!, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<CustomerOutput>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customerResult = await _customerRepository.GetByIdAsync(id, cancellationToken);
        if (customerResult.IsFailure)
            return Result<CustomerOutput>.Failure(customerResult.Errors);

        return customerResult.Value!.ToOutput();
    }

    public async Task<Result<List<CustomerOutput>>> GetPagedAsync(GetPagedInput input, CancellationToken cancellationToken = default)
    {
        var customersResult = await _customerRepository.GetPagedAsync(input.Page, input.PageSize, cancellationToken);
        if (customersResult.IsFailure)
            return Result<List<CustomerOutput>>.Failure(customersResult.Errors);

        return customersResult.Value!.ToOutput();
    }

    public async Task<Result<List<CustomerOutput>>> GetByNameAsync(string searchTerm, GetPagedInput input, CancellationToken cancellationToken = default)
    {
        var customersResult = await _customerRepository.GetByNameAsync(searchTerm, input.Page, input.PageSize, cancellationToken);
        if (customersResult.IsFailure)
            return Result<List<CustomerOutput>>.Failure(customersResult.Errors);

        return customersResult.Value!.ToOutput();
    }

    private static (string? phone, string? areaCode, string? number) ParsePhone(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return (null, null, null);

        var digits = Regex.Replace(input, @"[^\d]", string.Empty);

        if (digits.Length == 11 || digits.Length == 10)
            return (digits, digits[..2], digits[2..]);

        return (null, null, null);
    }

    private static (string? document, CustomerDocumentType? documentType) ParseDocument(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return (null, null);

        var digits = Regex.Replace(input, @"[^\d]", string.Empty);
        if (digits.Length == 11) return (digits, CustomerDocumentType.Cpf);
        if (digits.Length == 14) return (digits, CustomerDocumentType.Cnpj);
        return (digits, null);
    }
}
