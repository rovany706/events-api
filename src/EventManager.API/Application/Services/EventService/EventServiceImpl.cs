using EventManager.API.Application.Services.EventService.Models;
using EventManager.API.Domain.DataAccess;
using EventManager.API.Domain.Repositories;
using EventManager.API.Models.Entities;
using EventManager.API.Models.Request;
using EventManager.API.Models.Response;
using EventManager.API.Models.Results;

using Microsoft.EntityFrameworkCore;

namespace EventManager.API.Application.Services.EventService;

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
    public async Task<PaginatedResult<Event>> GetEvents(EventFilterDto filterDto, PaginationParams paginationParams, CancellationToken ct)
    {
        var events = _eventRepository.GetEvents();
        var filteredEvents = FilterEvents(events, filterDto);
        var page = await PaginateResults(filteredEvents, paginationParams, ct);
        
        return page;
    }

    private static IQueryable<Event> FilterEvents(IQueryable<Event> events, EventFilterDto filterDto)
    {
        if (!string.IsNullOrWhiteSpace(filterDto.Title))
        {
            events = events.Where(e => e.Title.Contains(filterDto.Title, StringComparison.OrdinalIgnoreCase));
        }

        if (filterDto.From.HasValue)
        {
            events = events.Where(e => e.StartAt >= filterDto.From.Value);
        }

        if (filterDto.To.HasValue)
        {
            events = events.Where(e => e.EndAt <= filterDto.To.Value);
        }

        return events;
    }

    private static async Task<PaginatedResult<Event>> PaginateResults(IQueryable<Event> filteredEvents,
        PaginationParams paginationParams, CancellationToken ct)
    {
        var filteredCount = filteredEvents.Count();
        var totalPages = (int)Math.Ceiling((double)filteredCount / paginationParams.PageSize);
        var eventPage = await filteredEvents
            .OrderBy(e => e.Id)
            .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync(ct);

        return new PaginatedResult<Event>(eventPage, eventPage.Count, paginationParams.Page, totalPages, filteredCount);
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