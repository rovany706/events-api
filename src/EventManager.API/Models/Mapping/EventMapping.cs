using EventManager.API.Models.Entities;
using EventManager.API.Models.Request;
using EventManager.API.Models.Response;

namespace EventManager.API.Models.Mapping;

public static class EventMapping
{
    private const int UndefinedId = 0;
    private const int UndefinedTotalSeats = 0;

    public static Event ToEvent(this CreateEventRequest createEventRequest)
    {
        return new Event()
        {
            Id = UndefinedId,
            Title = createEventRequest.Title,
            Description = createEventRequest.Description,
            StartAt = createEventRequest.StartAt,
            EndAt = createEventRequest.EndAt,
            TotalSeats = createEventRequest.TotalSeats
        };
    }

    public static Event ToEvent(this UpdateEventRequest updateEventRequest, int idToAssign)
    {
        return new Event
        {
            Id = idToAssign,
            Title = updateEventRequest.Title,
            Description = updateEventRequest.Description,
            StartAt = updateEventRequest.StartAt,
            EndAt = updateEventRequest.EndAt,
            TotalSeats = 1
        };
    }

    public static EventInfoResponse ToEventResponse(this Event eventInfo)
    {
        return new EventInfoResponse
        {
            Id = eventInfo.Id,
            Title = eventInfo.Title,
            Description = eventInfo.Description,
            StartAt = eventInfo.StartAt,
            EndAt = eventInfo.EndAt,
            TotalSeats = eventInfo.TotalSeats,
            AvailableSeats = eventInfo.AvailableSeats
        };
    }
}
