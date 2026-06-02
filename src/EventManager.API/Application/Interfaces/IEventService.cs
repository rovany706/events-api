using EventManager.API.Models;

using System.Collections.ObjectModel;

namespace EventManager.API.Application.Interfaces;

/// <summary>
/// Интерфейс сервиса для работы с мероприятиями
/// </summary>
public interface IEventService
{
    /// <summary>
    /// Получение всех мероприятий
    /// </summary>
    /// <returns>Коллекция мероприятий</returns>
    ReadOnlyCollection<Event> GetEvents();

    /// <summary>
    /// Получение мероприятия по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор мероприятия</param>
    /// <returns>null, если мероприятие не найдено, иначе <see cref="Event" /></returns>
    Event? GetEventById(int id);

    /// <summary>
    /// Добавление мероприятия
    /// </summary>
    /// <param name="eventDto">Мероприятие</param>
    /// <returns>Идентификатор, присвоенный мероприятию</returns>
    int AddEvent(Event eventDto);

    /// <summary>
    /// Обновление информации о мероприятии
    /// </summary>
    /// <param name="eventDto">Мероприятие</param>
    /// <returns>false, если мероприятие не найдено, иначе true</returns>
    bool TryUpdateEvent(Event eventDto);

    /// <summary>
    /// Удаление мероприятия
    /// </summary>
    /// <param name="id">Идентификатор мероприятия</param>
    /// <returns>false, если мероприятие не найдено, иначе true</returns>
    bool TryRemoveEvent(int id);
}
