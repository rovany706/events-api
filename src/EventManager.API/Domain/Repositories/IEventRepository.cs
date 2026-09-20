using EventManager.API.Models.Entities;

namespace EventManager.API.Domain.Repositories;

/// <summary>
/// Интерфейс репозитория мероприятий
/// </summary>
public interface IEventRepository
{
    /// <summary>
    /// Получить все мероприятия
    /// </summary>
    IQueryable<Event> GetEvents();

    /// <summary>
    /// Получить мероприятие по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор мероприятия</param>
    /// <param name="ct">Токен отмены</param>
    Task<Event?> GetEventByIdAsync(int id, CancellationToken ct);

    /// <summary>
    /// Добавить мероприятие
    /// </summary>
    /// <param name="eventToAdd">Мероприятие</param>
    /// <param name="ct">Токен отмены</param>
    Task AddEventAsync(Event eventToAdd, CancellationToken ct);

    /// <summary>
    /// Удалить мероприятие
    /// </summary>
    /// <param name="eventToRemove">Мероприятие</param>
    void RemoveEvent(Event eventToRemove);
    
    /// <summary>
    /// Сохранить изменения
    /// </summary>
    /// <param name="ct">Токен отмены</param>
    Task SaveChangesAsync(CancellationToken ct);
}