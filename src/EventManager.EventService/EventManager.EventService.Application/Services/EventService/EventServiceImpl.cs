using System.Globalization;
using System.Text.Json;

using EventManager.EventService.Application.Abstractions.Caching;
using EventManager.EventService.Application.Abstractions.Persistence.Repositories;
using EventManager.EventService.Application.Abstractions.Services;
using EventManager.EventService.Application.Abstractions.Services.Dto;
using EventManager.EventService.Application.Common.Pagination;
using EventManager.EventService.Application.Common.Results;
using EventManager.EventService.Domain.Entities;

using Microsoft.Extensions.Logging;

namespace EventManager.EventService.Application.Services.EventService;

/// <summary>
/// Сервис для работы с мероприятиями
/// </summary>
public class EventServiceImpl : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly ICache _cache;
    private readonly ILogger<EventServiceImpl> _logger;

    public EventServiceImpl(IEventRepository eventRepository, ICache cache, ILogger<EventServiceImpl> logger)
    {
        _eventRepository = eventRepository;
        _cache = cache;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PaginatedResult<Event>> GetEvents(EventFilterDto filterDto, PaginationParamsDto paginationParams,
        CancellationToken ct)
    {
        var eventsPage = await _eventRepository.GetEvents(filterDto, paginationParams, ct);

        return eventsPage;
    }

    /// <inheritdoc />
    public async Task<Result<Event?>> GetEventById(int id, CancellationToken ct)
    {
        var cacheResult = await _cache.GetByKeyAsync($"event:{id}");

        if (cacheResult.IsSuccess)
        {
            var cachedEvent = JsonSerializer.Deserialize<Event>(cacheResult.Value);
            return cachedEvent;
        }

        var eventToGet = await _eventRepository.GetEventByIdAsync(id, ct);

        if (eventToGet == null)
        {
            _logger.LogDebug("Event with {eventId} not found.", id);
            return Result<Event?>.Failure(
                Error.NotFound(string.Format(CultureInfo.InvariantCulture, Resource.ErrorEventNotFound, id)));
        }

        await UpdateEventInCache(eventToGet);

        return Result<Event?>.Success(eventToGet);
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
        var eventResult = await GetEventById(eventId, ct);

        if (!eventResult.IsSuccess)
        {
            return eventResult;
        }

        var eventToUpdate = eventResult.Value!;
        eventToUpdate.Update(updateEventRequest.Title, updateEventRequest.Description, updateEventRequest.StartAt,
            updateEventRequest.EndAt);
        await _eventRepository.SaveChangesAsync(ct);

        await UpdateEventInCache(eventToUpdate);

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> RemoveEvent(int id, CancellationToken ct)
    {
        var eventResult = await GetEventById(id, ct);

        if (!eventResult.IsSuccess)
        {
            return eventResult;
        }

        var eventToRemove = eventResult.Value!;
        _eventRepository.RemoveEvent(eventToRemove);
        await _eventRepository.SaveChangesAsync(ct);

        await _cache.DeleteKeyAsync($"event:{id}");

        return Result.Success();
    }

    public async Task<List<Event>> GetTop10PopularEvents(CancellationToken ct)
    {
        var cachedResult = await _cache.GetByKeyAsync("events:top10");

        if (cachedResult.IsSuccess)
        {
            var cachedTop10List = JsonSerializer.Deserialize<List<Event>>(cachedResult.Value);

            if (cachedTop10List != null)
                return cachedTop10List;
        }

        var top10List = await _eventRepository.GetTop10PopularEvents(ct);
        await _cache.SetByKeyAsync("events:top10", JsonSerializer.Serialize(top10List),
            CacheTtlValues.GetTop10PopularEventsTtl);
        
        return top10List;
    }

    private async Task UpdateEventInCache(Event e)
    {
        await _cache.SetByKeyAsync($"event:{e.Id}", JsonSerializer.Serialize(e),
            CacheTtlValues.GetEventByIdTtl);
    }
}