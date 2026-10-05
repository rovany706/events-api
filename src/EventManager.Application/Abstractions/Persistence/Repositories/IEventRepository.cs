using EventManager.Application.Abstractions.Services.Dto;
using EventManager.Application.Common.Pagination;
using EventManager.Domain.Entities;

namespace EventManager.Application.Abstractions.Persistence.Repositories;

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
}