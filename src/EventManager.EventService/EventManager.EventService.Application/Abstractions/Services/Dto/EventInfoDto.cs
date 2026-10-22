using EventManager.EventService.Domain.Entities;

namespace EventManager.EventService.Application.Abstractions.Services.Dto;

public class EventInfoDto
{
    public EventInfoDto(int id,
        string title,
        string? description,
        DateTime startAt,
        DateTime endAt,
        int totalSeats,
        int availableSeats)
    {
        Id = id;
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
        TotalSeats = totalSeats;
        AvailableSeats = availableSeats;
    }
    
    public static EventInfoDto FromEvent(Event e)
    {
        return new EventInfoDto(e.Id, e.Title, e.Description, e.StartAt, e.EndAt, e.TotalSeats, e.AvailableSeats);
    } 

    /// <summary>
    /// Идентификатор мероприятия
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Название мероприятия
    /// </summary>
    public string Title { get; init; }

    /// <summary>
    /// Описание мероприятия
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Дата начала мероприятия
    /// </summary>
    public DateTime StartAt { get; init; }

    /// <summary>
    /// Дата конца мероприятия
    /// </summary>
    public DateTime EndAt { get; init; }

    /// <summary>
    /// Общее количество мест на событии
    /// </summary>
    public int TotalSeats { get; init; }

    /// <summary>
    /// Текущее количество свободных мест
    /// </summary>
    public int AvailableSeats { get; init; }
}