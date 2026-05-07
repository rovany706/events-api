using System.Collections.ObjectModel;

using EventManager.API.Application.Interfaces;
using EventManager.API.Models;

namespace EventManager.API.Application.Services;

/// <summary>
/// Сервис для работы с мероприятиями
/// </summary>
public class EventService : IEventService
{
    private static readonly List<EventDto> _events = [];

    /// <inheritdoc />
    public int AddEvent(EventDto eventDto)
    {
        var newId = _events.Any() ? _events.Max(x => x.Id) + 1 : 1;

        eventDto.Id = newId;
        _events.Add(eventDto);

        return newId;
    }

    /// <inheritdoc />
    public ReadOnlyCollection<EventDto> GetAllEvents()
    {
        return _events.AsReadOnly();
    }

    /// <inheritdoc />
    public EventDto? GetEventById(int id)
    {
        return _events.FirstOrDefault(x => x.Id == id);
    }

    /// <inheritdoc />
    public bool TryRemoveEvent(int id)
    {
        var eventToRemove = GetEventById(id);

        if (eventToRemove == null)
        {
            return false;
        }

        _events.Remove(eventToRemove);

        return true;
    }

    /// <inheritdoc />
    public bool TryUpdateEvent(EventDto eventDto)
    {
        var eventToUpdate = GetEventById(eventDto.Id);

        if (eventToUpdate == null)
        {
            return false;
        }

        eventToUpdate.Title = eventDto.Title;
        eventToUpdate.Description = eventDto.Description;
        eventToUpdate.StartAt = eventDto.StartAt;
        eventToUpdate.EndAt = eventDto.EndAt;

        return true;
    }
}
