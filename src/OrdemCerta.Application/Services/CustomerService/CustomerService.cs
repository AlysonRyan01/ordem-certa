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

    public async Task<Result<CustomerOutput>> CreateAsync(CreateCustomerInput input, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
            return string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));

        var nameResult = CustomerName.Create(input.FullName);
        if (nameResult.IsFailure)
            return nameResult.ErrorMessage!;

        var phoneResult = CustomerPhone.Create(input.Phone);
        if (phoneResult.IsFailure)
            return phoneResult.ErrorMessage!;

        CustomerEmail? email = null;
        if (!string.IsNullOrWhiteSpace(input.Email))
        {
            var emailResult = CustomerEmail.Create(input.Email);
            if (emailResult.IsFailure)
                return emailResult.ErrorMessage!;
            email = emailResult.Value;
        }

        CustomerDocument? document = null;
        if (!string.IsNullOrWhiteSpace(input.Document))
        {
            var documentResult = CustomerDocument.Create(input.Document);
            if (documentResult.IsFailure)
                return documentResult.ErrorMessage!;
            document = documentResult.Value;
        }

        CustomerAddress? address = null;
        if (!string.IsNullOrWhiteSpace(input.Street) || !string.IsNullOrWhiteSpace(input.City))
        {
            var addressResult = CustomerAddress.Create(input.Street, input.Number, input.City, input.State);
            if (addressResult.IsFailure)
                return addressResult.ErrorMessage!;
            address = addressResult.Value;
        }

        var customerResult = Customer.Create(
            nameResult.Value!,
            new List<CustomerPhone> { phoneResult.Value! },
            email,
            address,
            document
        );

        if (customerResult.IsFailure)
            return customerResult.ErrorMessage!;

        await _customerRepository.AddAsync(customerResult.Value!, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return customerResult.Value!.ToOutput();
    }

    public async Task<Result<CustomerOutput>> UpdateAsync(Guid id, UpdateCustomerInput input, CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
            return string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));

        var customerResult = await _customerRepository.GetByIdAsync(id, cancellationToken);
        if (customerResult.IsFailure)
            return customerResult.ErrorMessage!;

        var customer = customerResult.Value!;

        var nameResult = CustomerName.Create(input.FullName);
        if (nameResult.IsFailure)
            return nameResult.ErrorMessage!;

        var updateNameResult = customer.UpdateName(nameResult.Value!);
        if (updateNameResult.IsFailure)
            return updateNameResult.ErrorMessage!;

        if (!string.IsNullOrWhiteSpace(input.Email))
        {
            var emailResult = CustomerEmail.Create(input.Email);
            if (emailResult.IsFailure)
                return emailResult.ErrorMessage!;

            var updateEmailResult = customer.UpdateEmail(emailResult.Value);
            if (updateEmailResult.IsFailure)
                return updateEmailResult.ErrorMessage!;
        }

        if (!string.IsNullOrWhiteSpace(input.Street) || !string.IsNullOrWhiteSpace(input.City))
        {
            var addressResult = CustomerAddress.Create(input.Street, input.Number, input.City, input.State);
            if (addressResult.IsFailure)
                return addressResult.ErrorMessage!;

            var updateAddressResult = customer.UpdateAddress(addressResult.Value);
            if (updateAddressResult.IsFailure)
                return updateAddressResult.ErrorMessage!;
        }

        await _customerRepository.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return customer.ToOutput();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customerResult = await _customerRepository.GetByIdAsync(id, cancellationToken);
        if (customerResult.IsFailure)
            return customerResult.ErrorMessage!;

        await _customerRepository.DeleteAsync(customerResult.Value!, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<CustomerOutput>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customerResult = await _customerRepository.GetByIdAsync(id, cancellationToken);
        if (customerResult.IsFailure)
            return customerResult.ErrorMessage!;

        return customerResult.Value!.ToOutput();
    }

    public async Task<Result<List<CustomerOutput>>> GetPagedAsync(GetPagedInput input, CancellationToken cancellationToken = default)
    {
        var customersResult = await _customerRepository.GetPagedAsync(input.Page, input.PageSize, cancellationToken);
        if (customersResult.IsFailure)
            return customersResult.ErrorMessage!;

        return customersResult.Value!.ToOutput();
    }
    
    public async Task<Result<List<CustomerOutput>>> GetByNameAsync(string searchTerm, GetPagedInput input, CancellationToken cancellationToken = default)
    {
        var customersResult = await _customerRepository.GetByNameAsync(searchTerm, input.Page, input.PageSize, cancellationToken);
    
        if (customersResult.IsFailure)
            return customersResult.ErrorMessage!;

        return customersResult.Value!.ToOutput();
    }

    public async Task<Result> AddPhoneAsync(Guid id, AddPhoneInput input, CancellationToken cancellationToken = default)
    {
        var validationResult = await _addPhoneValidator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
            return Result.Failure(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var customerResult = await _customerRepository.GetByIdAsync(id, cancellationToken);
        if (customerResult.IsFailure)
            return Result.Failure(customerResult.ErrorMessage!);

        var customer = customerResult.Value!;

        var phoneResult = CustomerPhone.Create(input.Phone);
        if (phoneResult.IsFailure)
            return Result.Failure(phoneResult.ErrorMessage!);

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
            return Result.Failure(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var customerResult = await _customerRepository.GetByIdAsync(id, cancellationToken);
        if (customerResult.IsFailure)
            return Result.Failure(customerResult.ErrorMessage!);

        var customer = customerResult.Value!;

        var phoneResult = CustomerPhone.Create(input.Phone);
        if (phoneResult.IsFailure)
            return Result.Failure(phoneResult.ErrorMessage!);

        var removePhoneResult = customer.RemovePhone(phoneResult.Value!);
        if (removePhoneResult.IsFailure)
            return removePhoneResult;

        await _customerRepository.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}