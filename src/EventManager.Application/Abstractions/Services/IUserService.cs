using EventManager.Application.Abstractions.Services.Dto;
using EventManager.Application.Common.Results;
using EventManager.Domain.Entities.Users;

namespace EventManager.Application.Abstractions.Services;

public interface IUserService
{
    Task<Result> RegisterUserAsync(UserCredentialsDto credentialsDto, UserRole userRole,
        CancellationToken cancellationToken);

    Task<Result<string>> LoginUserAsync(UserCredentialsDto credentialsDto, CancellationToken cancellationToken);
}