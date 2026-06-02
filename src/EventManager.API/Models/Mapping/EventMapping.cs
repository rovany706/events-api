using EventManager.API.Models.Request;
using EventManager.API.Models.Response;

namespace EventManager.API.Models.Mapping;

public static class EventMapping
{
    public static Event ToEvent(this CreateEventRequest createEventRequest)
    {
        return new Event
        {
            Id = 0,
            Title = createEventRequest.Title,
            Description = createEventRequest.Description,
            StartAt = createEventRequest.StartAt,
            EndAt = createEventRequest.EndAt
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
            EndAt = updateEventRequest.EndAt
        };
    }

    public static EventResponse ToEventResponse(this Event eventInfo)
    {
        return new EventResponse
        {
            Id = eventInfo.Id,
            Title = eventInfo.Title,
            Description = eventInfo.Description,
            StartAt = eventInfo.StartAt,
            EndAt = eventInfo.EndAt
        };
    }
}
