using EventManager.API.Application.Services.EventService.Models;
using EventManager.API.Domain.Interfaces;
using EventManager.API.Models;
using EventManager.API.Models.Request;
using EventManager.API.Models.Response;

namespace EventManager.API.Application.Services.EventService;

/// <summary>
/// Сервис для работы с мероприятиями
/// </summary>
public class EventServiceImpl : IEventService
{
    private readonly IEventRepository _eventRepository;

    public EventServiceImpl(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    /// <inheritdoc />
    public PaginatedResult<Event> GetEvents(EventFilterDto filterDto, PaginationParams paginationParams)
    {
        var events = _eventRepository.GetEvents();
        var filteredEvents = FilterEvents(events, filterDto);
        return PaginateResults(filteredEvents, paginationParams);
    }

    private static IEnumerable<Event> FilterEvents(IEnumerable<Event> events, EventFilterDto filterDto)
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

    private static PaginatedResult<Event> PaginateResults(IEnumerable<Event> filteredEvents, PaginationParams paginationParams)
    {
        var filteredCount = filteredEvents.Count();
        var totalPages = (int)Math.Ceiling((double)filteredCount / paginationParams.PageSize);
        var eventPage = filteredEvents
            .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToList();

        return new PaginatedResult<Event>(eventPage, eventPage.Count, paginationParams.Page, totalPages, filteredCount);
    }

    /// <inheritdoc />
    public Event? GetEventById(int id)
    {
        return _eventRepository.GetEventById(id);
    }

    /// <inheritdoc />
    public int AddEvent(Event eventToAdd)
    {
        var newId = _eventRepository.AddEvent(eventToAdd);

        return newId;
    }

    /// <inheritdoc />
    public bool TryUpdateEvent(Event eventToUpdate)
    {
        var isEventExist = GetEventById(eventToUpdate.Id) != null;

        if (!isEventExist)
        {
            return false;
        }

        _eventRepository.UpdateEvent(eventToUpdate);

        return true;
    }

    /// <inheritdoc />
    public bool TryRemoveEvent(int id)
    {
        var eventToRemove = GetEventById(id);

        if (eventToRemove == null)
        {
            return false;
        }

        return _eventRepository.RemoveEvent(eventToRemove);
    }
}
