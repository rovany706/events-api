using EventManager.EventService.Domain.Entities;
using EventManager.EventService.Presentation.Models.Response;

namespace EventManager.EventService.Presentation.Models.Mapping;

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