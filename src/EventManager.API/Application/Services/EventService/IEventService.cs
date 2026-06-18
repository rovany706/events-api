using EventManager.API.Application.Services.EventService.Models;
using EventManager.API.Models;
using EventManager.API.Models.Request;
using EventManager.API.Models.Response;

namespace EventManager.API.Application.Services.EventService;

/// <summary>
/// Интерфейс сервиса для работы с мероприятиями
/// </summary>
public interface IEventService
{
    /// <summary>
    /// Получение всех мероприятий
    /// </summary>
    /// <param name="filterDto">Параметры фильтрации</param>
    /// <param name="paginationParams">Параметры пагинации</param>
    /// <returns>Коллекция мероприятий с пагинацией</returns>
    PaginatedResult<Event> GetEvents(EventFilterDto filterDto, PaginationParams paginationParams);

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
