using FluentValidation;
using OrdemCerta.Application.Abstractions;
using OrdemCerta.Application.Inputs.UserInputs;
using OrdemCerta.Domain.Users;
using OrdemCerta.Domain.Users.DTOs;
using OrdemCerta.Domain.Users.Extensions;
using OrdemCerta.Domain.Users.ValueObjects;
using OrdemCerta.Infrastructure.DataContext.Uow;
using OrdemCerta.Infrastructure.Repositories.UserRepository;
using OrdemCerta.Shared;

namespace OrdemCerta.Application.Services.UserService;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<RegisterUserInput> _registerValidator;
    private readonly IValidator<UpdateUserInput> _updateValidator;

    public UserService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IValidator<RegisterUserInput> registerValidator,
        IValidator<UpdateUserInput> updateValidator)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _registerValidator = registerValidator;
        _updateValidator = updateValidator;
    }

    public async Task<Result<UserOutput>> RegisterAsync(RegisterUserInput input, CancellationToken cancellationToken)
    {
        var validationResult = await _registerValidator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
            return Result<UserOutput>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var alreadyExists = await _userRepository.ExistsByCompanyIdAsync(input.CompanyId, cancellationToken);
        if (alreadyExists)
            return "Já existe um usuário cadastrado para esta empresa";

        var nameResult = UserName.Create(input.Name);
        if (nameResult.IsFailure)
            return Result<UserOutput>.Failure(nameResult.Errors);

        var emailResult = UserEmail.Create(input.Email);
        if (emailResult.IsFailure)
            return Result<UserOutput>.Failure(emailResult.Errors);

        var passwordHash = _passwordHasher.Hash(input.Password);

        var userResult = User.Create(input.CompanyId, nameResult.Value!, emailResult.Value!, passwordHash);
        if (userResult.IsFailure)
            return Result<UserOutput>.Failure(userResult.Errors);

        await _userRepository.AddAsync(userResult.Value!, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return userResult.Value!.ToOutput();
    }

    public async Task<Result<UserOutput>> UpdateAsync(Guid id, UpdateUserInput input, CancellationToken cancellationToken)
    {
        var validationResult = await _updateValidator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
            return Result<UserOutput>.Failure(validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var userResult = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (userResult.IsFailure)
            return Result<UserOutput>.Failure(userResult.Errors);

        var user = userResult.Value!;

        var nameResult = UserName.Create(input.Name);
        if (nameResult.IsFailure)
            return Result<UserOutput>.Failure(nameResult.Errors);

        user.UpdateName(nameResult.Value!);

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return user.ToOutput();
    }

    public async Task<Result<UserOutput>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var userResult = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (userResult.IsFailure)
            return Result<UserOutput>.Failure(userResult.Errors);

        return userResult.Value!.ToOutput();
    }
}
