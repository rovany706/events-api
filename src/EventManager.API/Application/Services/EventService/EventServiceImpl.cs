using EventManager.API.Application.Services.EventService.Models;
using EventManager.API.Domain.Interfaces;
using EventManager.API.Models.Entities;
using EventManager.API.Models.Request;
using EventManager.API.Models.Response;
using EventManager.API.Models.Results;

namespace EventManager.API.Application.Services.EventService;

/// <summary>
/// Сервис для работы с мероприятиями
/// </summary>
public class EventServiceImpl : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly ILogger<EventServiceImpl> _logger;

    public EventServiceImpl(IEventRepository eventRepository, IBookingRepository bookingRepository,
        ILogger<EventServiceImpl> logger)
    {
        _eventRepository = eventRepository;
        _bookingRepository = bookingRepository;
        _logger = logger;
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

    private static PaginatedResult<Event> PaginateResults(IEnumerable<Event> filteredEvents,
        PaginationParams paginationParams)
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
    public Result<Event?> GetEventById(int id)
    {
        var eventToGet = _eventRepository.GetEventById(id);

        if (eventToGet == null)
        {
            _logger.LogDebug("Event with {eventId} not found.", id);
            return Result<Event?>.Failure(Error.NotFound($"Event with {id} not found."));
        }

        return Result<Event?>.Success(eventToGet);
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
        var eventResult = GetEventById(eventToUpdate.Id);

        if (!eventResult.IsSuccess)
        {
            return false;
        }

        _eventRepository.UpdateEvent(eventToUpdate);

        return true;
    }

    /// <inheritdoc />
    public bool TryRemoveEvent(int id)
    {
        var eventResult = GetEventById(id);

        if (!eventResult.IsSuccess)
        {
            return false;
        }

        var eventToRemove = eventResult.Value!;
        var result = _eventRepository.RemoveEvent(eventToRemove);
        result &= RemoveEventBookings(id);

        return result;
    }

    private bool RemoveEventBookings(int eventId)
    {
        var result = true;
        var bookings = _bookingRepository.GetBookings().Where(x => x.EventId == eventId).ToList();

        foreach (var booking in bookings)
        {
            result &= _bookingRepository.RemoveBooking(booking);
        }

        return result;
    }
}