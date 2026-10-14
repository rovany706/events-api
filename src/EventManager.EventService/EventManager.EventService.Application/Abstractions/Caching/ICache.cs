using EventManager.EventService.Application.Common.Results;

namespace EventManager.EventService.Application.Abstractions.Caching;

/// <summary>
/// Интерфейс кеширования
/// </summary>
public interface ICache
{
    /// <summary>
    /// Получить значение по ключу
    /// </summary>
    /// <param name="key">Ключ</param>
    Task<Result<string>> GetByKeyAsync(string key);

    /// <summary>
    /// Задать значение
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="value">Значение</param>
    /// <param name="ttl">Время жизни</param>
    /// <returns></returns>
    Task<Result> SetByKeyAsync(string key, string value, TimeSpan ttl);

    /// <summary>
    /// Удалить ключ
    /// </summary>
    /// <param name="key">Ключ</param>
    Task<Result> DeleteKeyAsync(string key);
}