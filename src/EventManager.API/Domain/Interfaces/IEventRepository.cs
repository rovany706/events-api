using System.Collections.ObjectModel;

using EventManager.API.Models;

namespace EventManager.API.Domain.Interfaces;

/// <summary>
/// Интерфейс репозитория мероприятий
/// </summary>
public interface IEventRepository
{
    /// <summary>
    /// Получить все мероприятия
    /// </summary>
    ReadOnlyCollection<Event> GetEvents();

    /// <summary>
    /// Получить мероприятие по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор мероприятия</param>
    Event? GetEventById(int id);

    /// <summary>
    /// Добавить мероприятие
    /// </summary>
    /// <param name="eventToAdd">Мероприятие</param>
    /// <returns>Идентификатор, присвоенный мероприятию</returns>
    int AddEvent(Event eventToAdd);

    /// <summary>
    /// Обновить мероприятие
    /// </summary>
    /// <param name="updatedEvent">Обновленное мероприятие</param>
    void UpdateEvent(Event updatedEvent);

    /// <summary>
    /// Удалить мероприятие
    /// </summary>
    /// <param name="eventToRemove">Мероприятие</param>
    /// <returns>false - мероприятие не удалось удалить, иначе true</returns>
    bool RemoveEvent(Event eventToRemove);
}
