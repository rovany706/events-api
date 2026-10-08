using EventManager.Domain.Entities;
using EventManager.Presentation.Models.Response;

namespace EventManager.Presentation.Models.Mapping;

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