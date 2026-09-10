using EventManager.API.Models.Entities;
using EventManager.API.Models.Response;

namespace EventManager.API.Models.Mapping;

public static class EventMapping
{
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
