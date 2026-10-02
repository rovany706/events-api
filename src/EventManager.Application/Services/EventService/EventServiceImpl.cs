using EventManager.Application.Abstractions.Persistence.Repositories;
using EventManager.Application.Abstractions.Services;
using EventManager.Application.Abstractions.Services.Dto;
using EventManager.Application.Common.Pagination;
using EventManager.Application.Common.Results;
using EventManager.Domain.Entities;

using Microsoft.Extensions.Logging;

namespace EventManager.Application.Services.EventService;

/// <summary>
/// Сервис для работы с мероприятиями
/// </summary>
public class EventServiceImpl : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly ILogger<EventServiceImpl> _logger;

    public EventServiceImpl(IEventRepository eventRepository, ILogger<EventServiceImpl> logger)
    {
        _eventRepository = eventRepository;
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
        var eventToGet = await _eventRepository.GetEventByIdAsync(id, ct);

        if (eventToGet == null)
        {
            _logger.LogDebug("Event with {eventId} not found.", id);
            return Result<Event?>.Failure(Error.NotFound($"Event with {id} not found."));
        }

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
    public async Task<bool> TryUpdateEvent(int eventId, UpdateEventRequest updateEventRequest, CancellationToken ct)
    {
        var eventResult = await GetEventById(eventId, ct);

        if (!eventResult.IsSuccess)
        {
            return false;
        }

        var eventToUpdate = eventResult.Value!;
        eventToUpdate.Update(updateEventRequest.Title, updateEventRequest.Description, updateEventRequest.StartAt,
            updateEventRequest.EndAt);
        await _eventRepository.SaveChangesAsync(ct);

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> TryRemoveEvent(int id, CancellationToken ct)
    {
        var eventResult = await GetEventById(id, ct);

        if (!eventResult.IsSuccess)
        {
            return false;
        }

        var eventToRemove = eventResult.Value!;
        _eventRepository.RemoveEvent(eventToRemove);
        await _eventRepository.SaveChangesAsync(ct);

        return true;
    }
}