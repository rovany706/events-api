using EventManager.UserService.Application.Abstractions.Services.Dto;
using EventManager.UserService.Application.Common.Results;
using EventManager.UserService.Domain.Entities;

namespace EventManager.UserService.Application.Abstractions.Services;

public interface IUserService
{
    /// <summary>
    /// Регистрация пользователя
    /// </summary>
    /// <param name="credentialsDto">Данные для входа</param>
    /// <param name="userRole">Роль пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns></returns>
    Task<Result> RegisterUserAsync(UserCredentialsDto credentialsDto, UserRole userRole,
        CancellationToken cancellationToken);

    /// <summary>
    /// Вход пользователя
    /// </summary>
    /// <param name="credentialsDto">Данные для входа</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns></returns>
    Task<Result<string>> LoginUserAsync(UserCredentialsDto credentialsDto, CancellationToken cancellationToken);
}