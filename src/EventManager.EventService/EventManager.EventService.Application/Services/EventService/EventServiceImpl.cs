using System.Globalization;
using System.Text.Json;

using EventManager.EventService.Application.Abstractions.Caching;
using EventManager.EventService.Application.Abstractions.Persistence.Repositories;
using EventManager.EventService.Application.Abstractions.Services;
using EventManager.EventService.Application.Abstractions.Services.Dto;
using EventManager.EventService.Application.Common.Pagination;
using EventManager.EventService.Application.Common.Results;
using EventManager.EventService.Application.Options;
using EventManager.EventService.Domain.Entities;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventManager.EventService.Application.Services.EventService;

/// <summary>
/// Сервис для работы с мероприятиями
/// </summary>
public class EventServiceImpl : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly ICache _cache;
    private readonly CacheSettings _cacheSettings;
    private readonly ILogger<EventServiceImpl> _logger;

    public EventServiceImpl(IEventRepository eventRepository, ICache cache,
        IOptions<CacheSettings> cacheSettings, ILogger<EventServiceImpl> logger)
    {
        _eventRepository = eventRepository;
        _cache = cache;
        _cacheSettings = cacheSettings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaginatedResult<EventInfoDto>> GetEvents(EventFilterDto filterDto,
        PaginationParamsDto paginationParams,
        CancellationToken ct)
    {
        var eventsPage = await _eventRepository.GetEvents(filterDto, paginationParams, ct);

        return new PaginatedResult<EventInfoDto>(
            eventsPage.Items.Select(EventInfoDto.FromEvent).ToList(),
            eventsPage.ItemCount,
            eventsPage.CurrentPage,
            eventsPage.TotalPages,
            eventsPage.TotalItems);
    }

    /// <inheritdoc />
    public async Task<Result<EventInfoDto?>> GetEventById(int id, CancellationToken ct)
    {
        var cacheResult = await _cache.GetByKeyAsync($"event:{id}");

        if (cacheResult.IsSuccess)
        {
            var cachedEvent = JsonSerializer.Deserialize<EventInfoDto>(cacheResult.Value);

            if (cachedEvent != null)
                return cachedEvent;
        }

        var eventToGet = await _eventRepository.GetEventByIdAsync(id, ct);

        if (eventToGet == null)
        {
            _logger.LogDebug("Event with {eventId} not found.", id);
            return Result<EventInfoDto?>.Failure(
                Error.NotFound(string.Format(CultureInfo.InvariantCulture, Resource.ErrorEventNotFound, id)));
        }

        var dto = EventInfoDto.FromEvent(eventToGet);
        await UpdateEventInCache(dto);

        return Result<EventInfoDto?>.Success(dto);
    }

    /// <inheritdoc />
    public async Task<int> AddEvent(CreateEventRequest createEventRequest, CancellationToken ct)
    {
        var newEvent = Event.CreateInstance(createEventRequest.Title, createEventRequest.Description,
            createEventRequest.StartAt, createEventRequest.EndAt, createEventRequest.TotalSeats);

        await _eventRepository.AddEventAsync(newEvent, ct);
        await _eventRepository.SaveChangesAsync(ct);

        return newEvent.Id;
    }

    /// <inheritdoc />
    public async Task<Result> UpdateEvent(int eventId, UpdateEventRequest updateEventRequest, CancellationToken ct)
    {
        var eventToUpdate = await _eventRepository.GetEventByIdAsync(eventId, ct);

        if (eventToUpdate == null)
        {
            _logger.LogDebug("Event with {eventId} not found.", eventId);
            return Result<EventInfoDto?>.Failure(
                Error.NotFound(string.Format(CultureInfo.InvariantCulture, Resource.ErrorEventNotFound, eventId)));
        }

        eventToUpdate.Update(updateEventRequest.Title, updateEventRequest.Description, updateEventRequest.StartAt,
            updateEventRequest.EndAt);
        await _eventRepository.SaveChangesAsync(ct);

        await UpdateEventInCache(EventInfoDto.FromEvent(eventToUpdate));

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> RemoveEvent(int id, CancellationToken ct)
    {
        var eventToRemove = await _eventRepository.GetEventByIdAsync(id, ct);

        if (eventToRemove == null)
        {
            _logger.LogDebug("Event with {eventId} not found.", id);
            return Result<EventInfoDto?>.Failure(
                Error.NotFound(string.Format(CultureInfo.InvariantCulture, Resource.ErrorEventNotFound, id)));
        }

        _eventRepository.RemoveEvent(eventToRemove);
        await _eventRepository.SaveChangesAsync(ct);

        await _cache.DeleteKeyAsync($"event:{id}");

        return Result.Success();
    }

    public async Task<List<EventInfoDto>> GetTop10PopularEvents(CancellationToken ct)
    {
        var cachedResult = await _cache.GetByKeyAsync("events:top10");

        if (cachedResult.IsSuccess)
        {
            var cachedTop10List = JsonSerializer.Deserialize<List<EventInfoDto>>(cachedResult.Value);

            if (cachedTop10List != null)
                return cachedTop10List;
        }

        var top10List = (await _eventRepository.GetTop10PopularEvents(ct))
            .Select(EventInfoDto.FromEvent)
            .ToList();
        await _cache.SetByKeyAsync("events:top10", JsonSerializer.Serialize(top10List),
            _cacheSettings.TopEventsTtl);

        return top10List;
    }

    private async Task UpdateEventInCache(EventInfoDto e)
    {
        await _cache.SetByKeyAsync($"event:{e.Id}", JsonSerializer.Serialize(e),
            _cacheSettings.EventTtl);
    }
}