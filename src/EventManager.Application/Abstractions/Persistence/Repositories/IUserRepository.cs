using EventManager.Domain.Entities.Users;

namespace EventManager.Application.Abstractions.Persistence.Repositories;

/// <summary>
/// Интерфейс репозитория пользователей
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Получить пользователя по логину
    /// </summary>
    /// <param name="login">Логин пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns></returns>
    Task<User?> GetUserByLoginAsync(string login, CancellationToken cancellationToken);
    
    /// <summary>
    /// Получить пользователя по идентификатору
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns></returns>
    Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken);

    /// <summary>
    /// Добавить пользователя
    /// </summary>
    /// <param name="user">Пользователь</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns></returns>
    Task AddUserAsync(User user, CancellationToken cancellationToken);

    /// <summary>
    /// Сохранить изменения
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}