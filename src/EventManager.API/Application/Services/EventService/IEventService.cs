using EventManager.API.Application.Services.EventService.Models;
using EventManager.API.Models.Entities;
using EventManager.API.Models.Request;
using EventManager.API.Models.Response;
using EventManager.API.Models.Results;

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
    /// <param name="ct">Токен отмены</param>
    /// <returns>Коллекция мероприятий с пагинацией</returns>
    Task<PaginatedResult<Event>> GetEvents(EventFilterDto filterDto, PaginationParams paginationParams,
        CancellationToken ct);

    /// <summary>
    /// Получение мероприятия по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор мероприятия</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>null, если мероприятие не найдено, иначе <see cref="Event" /></returns>
    Task<Result<Event?>> GetEventById(int id, CancellationToken ct);

    /// <summary>
    /// Добавление мероприятия
    /// </summary>
    /// <param name="createEventRequest">Информация о создании мероприятия</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Идентификатор, присвоенный мероприятию</returns>
    Task<int> AddEvent(CreateEventRequest createEventRequest, CancellationToken ct);

    /// <summary>
    /// Обновление информации о мероприятии
    /// </summary>
    /// <param name="eventId">Идентификатор мероприятия</param>
    /// <param name="updateEventRequest">Информация об обновлении</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>false, если мероприятие не найдено, иначе true</returns>
    Task<bool> TryUpdateEvent(int eventId, UpdateEventRequest updateEventRequest, CancellationToken ct);

    /// <summary>
    /// Удаление мероприятия
    /// </summary>
    /// <param name="id">Идентификатор мероприятия</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>false, если мероприятие не найдено, иначе true</returns>
    Task<bool> TryRemoveEvent(int id, CancellationToken ct);
}
