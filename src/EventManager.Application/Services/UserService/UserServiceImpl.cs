using EventManager.Application.Abstractions.Persistence.Repositories;
using EventManager.Application.Abstractions.Security;
using EventManager.Application.Abstractions.Services;
using EventManager.Application.Abstractions.Services.Dto;
using EventManager.Application.Common.Results;
using EventManager.Domain.Entities.Users;

namespace EventManager.Application.Services.UserService;

public class UserServiceImpl : IUserService
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUserRepository _userRepository;

    public UserServiceImpl(IPasswordHasher passwordHasher, IUserJwtTokenGenerator jwtTokenGenerator,
        IUserRepository userRepository)
    {
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _userRepository = userRepository;
    }

    public async Task<Result> RegisterUserAsync(UserCredentialsDto credentialsDto, UserRole userRole,
        CancellationToken cancellationToken)
    {
        var isLoginTaken = (await _userRepository.GetUserByLoginAsync(credentialsDto.Login, cancellationToken)) != null;
        if (isLoginTaken)
            return Result<int>.Failure(Error.Conflict($"Login {credentialsDto.Login} is taken."));

        var passwordHash = _passwordHasher.Hash(credentialsDto.Password);
        var newUser = User.CreateInstance(credentialsDto.Login, passwordHash, userRole);
        await _userRepository.AddUserAsync(newUser, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<string>> LoginUserAsync(UserCredentialsDto credentialsDto,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByLoginAsync(credentialsDto.Login, cancellationToken);
        if (user == null)
            return Result<string>.Failure(Error.Unauthorized("Invalid login or password."));

        var passwordHash = _passwordHasher.Hash(credentialsDto.Password);
        if (user.PasswordHash != passwordHash)
            return Result<string>.Failure(Error.Unauthorized("Invalid login or password."));

        var token = _jwtTokenGenerator.CreateToken(user);

        return token;
    }
}