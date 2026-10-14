using EventManager.EventService.Application.Abstractions.Services.Dto;
using EventManager.EventService.Application.Common.Pagination;
using EventManager.EventService.Domain.Entities;

namespace EventManager.EventService.Application.Abstractions.Persistence.Repositories;

/// <summary>
///     Интерфейс репозитория мероприятий
/// </summary>
public interface IEventRepository
{
    /// <summary>
    /// Получить все мероприятия
    /// </summary>
    /// <param name="filterDto">Параметры фильтрации</param>
    /// <param name="paginationParams">Параметры пагинации</param>
    /// <param name="ct">Токен отмены</param>
    Task<PaginatedResult<Event>> GetEvents(EventFilterDto? filterDto = null,
        PaginationParamsDto? paginationParams = null, CancellationToken ct = default);

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

    /// <summary>
    /// Получить топ-10 самых популярных мероприятий
    /// </summary>
    /// <param name="ct">Токен отмены</param>
    Task<List<Event>> GetTop10PopularEvents(CancellationToken ct);
}