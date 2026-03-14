using FluentValidation;
using OrdemCerta.Application.Inputs.CustomerInputs;
using OrdemCerta.Domain.Customers;
using OrdemCerta.Domain.Customers.DTOs;
using OrdemCerta.Domain.Customers.Extensions;
using OrdemCerta.Domain.Customers.ValueObjects;
using OrdemCerta.Infrastructure.DataContext.Uow;
using OrdemCerta.Infrastructure.Repositories.CustomerRepository;
using OrdemCerta.Shared;

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

        var nameResult = CustomerName.Create(input.FullName);
        if (nameResult.IsFailure)
            return Result<CustomerOutput>.Failure(nameResult.Errors);

        var phoneResult = CustomerPhone.Create(input.Phone);
        if (phoneResult.IsFailure)
            return Result<CustomerOutput>.Failure(phoneResult.Errors);

        CustomerEmail? email = null;
        if (!string.IsNullOrWhiteSpace(input.Email))
        {
            var emailResult = CustomerEmail.Create(input.Email);
            if (emailResult.IsFailure)
                return Result<CustomerOutput>.Failure(emailResult.Errors);

            email = emailResult.Value;
        }

        CustomerDocument? document = null;
        if (!string.IsNullOrWhiteSpace(input.Document))
        {
            var documentResult = CustomerDocument.Create(input.Document);
            if (documentResult.IsFailure)
                return Result<CustomerOutput>.Failure(documentResult.Errors);

            document = documentResult.Value;
        }

        CustomerAddress? address = null;
        if (!string.IsNullOrWhiteSpace(input.Street) || !string.IsNullOrWhiteSpace(input.City))
        {
            var addressResult = CustomerAddress.Create(input.Street, input.Number, input.City, input.State);
            if (addressResult.IsFailure)
                return Result<CustomerOutput>.Failure(addressResult.Errors);

            address = addressResult.Value;
        }

        var customerResult = Customer.Create(
            companyId,
            nameResult.Value!,
            new List<CustomerPhone> { phoneResult.Value! },
            email,
            address,
            document
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

        var customerResult = await _customerRepository.GetByIdAsync(id, cancellationToken);
        if (customerResult.IsFailure)
            return Result<CustomerOutput>.Failure(customerResult.Errors);

        var customer = customerResult.Value!;

        var nameResult = CustomerName.Create(input.FullName);
        if (nameResult.IsFailure)
            return Result<CustomerOutput>.Failure(nameResult.Errors);

        var updateNameResult = customer.UpdateName(nameResult.Value!);
        if (updateNameResult.IsFailure)
            return Result<CustomerOutput>.Failure(updateNameResult.Errors);

        if (!string.IsNullOrWhiteSpace(input.Email))
        {
            var emailResult = CustomerEmail.Create(input.Email);
            if (emailResult.IsFailure)
                return Result<CustomerOutput>.Failure(emailResult.Errors);

            var updateEmailResult = customer.UpdateEmail(emailResult.Value);
            if (updateEmailResult.IsFailure)
                return Result<CustomerOutput>.Failure(updateEmailResult.Errors);
        }

        if (!string.IsNullOrWhiteSpace(input.Street) || !string.IsNullOrWhiteSpace(input.City))
        {
            var addressResult = CustomerAddress.Create(input.Street, input.Number, input.City, input.State);
            if (addressResult.IsFailure)
                return Result<CustomerOutput>.Failure(addressResult.Errors);

            var updateAddressResult = customer.UpdateAddress(addressResult.Value);
            if (updateAddressResult.IsFailure)
                return Result<CustomerOutput>.Failure(updateAddressResult.Errors);
        }

        await _customerRepository.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return customer.ToOutput();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customerResult = await _customerRepository.GetByIdAsync(id, cancellationToken);
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

    public async Task<Result> AddPhoneAsync(Guid id, AddPhoneInput input, CancellationToken cancellationToken = default)
    {
        var validationResult = await _addPhoneValidator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result.Failure(errors);
        }

        var customerResult = await _customerRepository.GetByIdAsync(id, cancellationToken);
        if (customerResult.IsFailure)
            return Result.Failure(customerResult.Errors);

        var customer = customerResult.Value!;

        var phoneResult = CustomerPhone.Create(input.Phone);
        if (phoneResult.IsFailure)
            return Result.Failure(phoneResult.Errors);

        var addPhoneResult = customer.AddPhone(phoneResult.Value!);
        if (addPhoneResult.IsFailure)
            return addPhoneResult;

        await _customerRepository.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RemovePhoneAsync(Guid id, RemovePhoneInput input, CancellationToken cancellationToken = default)
    {
        var validationResult = await _removePhoneValidator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result.Failure(errors);
        }

        var customerResult = await _customerRepository.GetByIdAsync(id, cancellationToken);
        if (customerResult.IsFailure)
            return Result.Failure(customerResult.Errors);

        var customer = customerResult.Value!;

        var phoneResult = CustomerPhone.Create(input.Phone);
        if (phoneResult.IsFailure)
            return Result.Failure(phoneResult.Errors);

        var removePhoneResult = customer.RemovePhone(phoneResult.Value!);
        if (removePhoneResult.IsFailure)
            return removePhoneResult;

        await _customerRepository.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}