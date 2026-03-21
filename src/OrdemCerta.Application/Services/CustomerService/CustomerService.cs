using FluentValidation;
using OrdemCerta.Application.Inputs.CustomerInputs;
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
    private readonly IValidator<CreateCustomerInput> _createValidator;
    private readonly IValidator<UpdateCustomerInput> _updateValidator;
    private readonly IValidator<AddPhoneInput> _addPhoneValidator;
    private readonly IValidator<RemovePhoneInput> _removePhoneValidator;

    public CustomerService(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateCustomerInput> createValidator,
        IValidator<UpdateCustomerInput> updateValidator,
        IValidator<AddPhoneInput> addPhoneValidator,
        IValidator<RemovePhoneInput> removePhoneValidator)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _addPhoneValidator = addPhoneValidator;
        _removePhoneValidator = removePhoneValidator;
    }

    public async Task<Result<CustomerOutput>> CreateAsync(Guid companyId, CreateCustomerInput input, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result<CustomerOutput>.Failure(errors);
        }

        var phoneResult = CustomerPhone.Create(input.Phone);
        if (phoneResult.IsFailure)
            return Result<CustomerOutput>.Failure(phoneResult.Errors);

        (string? document, CustomerDocumentType? documentType) = ParseDocument(input.Document);

        var customerResult = Customer.Create(
            companyId,
            input.FullName,
            new List<CustomerPhone> { phoneResult.Value! },
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
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result<CustomerOutput>.Failure(errors);
        }

        var customerResult = await _customerRepository.GetByIdTrackedAsync(id, cancellationToken);
        if (customerResult.IsFailure)
            return Result<CustomerOutput>.Failure(customerResult.Errors);

        var customer = customerResult.Value!;

        customer.UpdateName(input.FullName);

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

    public async Task<Result<CustomerOutput>> AddPhoneAsync(Guid id, AddPhoneInput input, CancellationToken cancellationToken = default)
    {
        var validationResult = await _addPhoneValidator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
            return Result<CustomerOutput>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var customerResult = await _customerRepository.GetByIdTrackedAsync(id, cancellationToken);
        if (customerResult.IsFailure)
            return Result<CustomerOutput>.Failure(customerResult.Errors);

        var customer = customerResult.Value!;

        var phoneResult = CustomerPhone.Create(input.Phone);
        if (phoneResult.IsFailure)
            return Result<CustomerOutput>.Failure(phoneResult.Errors);

        var addPhoneResult = customer.AddPhone(phoneResult.Value!);
        if (addPhoneResult.IsFailure)
            return Result<CustomerOutput>.Failure(addPhoneResult.Errors);

        await _customerRepository.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return customer.ToOutput();
    }

    public async Task<Result<CustomerOutput>> RemovePhoneAsync(Guid id, RemovePhoneInput input, CancellationToken cancellationToken = default)
    {
        var validationResult = await _removePhoneValidator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
            return Result<CustomerOutput>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var customerResult = await _customerRepository.GetByIdTrackedAsync(id, cancellationToken);
        if (customerResult.IsFailure)
            return Result<CustomerOutput>.Failure(customerResult.Errors);

        var customer = customerResult.Value!;

        var phoneResult = CustomerPhone.Create(input.Phone);
        if (phoneResult.IsFailure)
            return Result<CustomerOutput>.Failure(phoneResult.Errors);

        var removePhoneResult = customer.RemovePhone(phoneResult.Value!);
        if (removePhoneResult.IsFailure)
            return Result<CustomerOutput>.Failure(removePhoneResult.Errors);

        await _customerRepository.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return customer.ToOutput();
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
