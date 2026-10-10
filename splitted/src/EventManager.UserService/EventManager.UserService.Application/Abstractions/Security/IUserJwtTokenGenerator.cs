using EventManager.UserService.Domain.Entities;

namespace EventManager.UserService.Application.Abstractions.Security;

/// <summary>
/// Интерфейс генератора JWT-токенов для пользователей
/// </summary>
public interface IUserJwtTokenGenerator
{
    /// <summary>
    /// Создать JWT-токен
    /// </summary>
    /// <param name="user">Пользователь</param>
    string CreateToken(User user);
}