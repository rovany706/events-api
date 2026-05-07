using System.Collections.ObjectModel;

using EventManager.API.Domain.Interfaces;
using EventManager.API.Models;

namespace EventManager.API.Domain.Repositories;

/// <summary>
/// Репозиторий мероприятий
/// </summary>
public class InMemoryEventRepository : IEventRepository
{
    private static readonly List<Event> _events = [];
    private int _nextId = 1;

    /// <inheritdoc />
    public Event? GetEventById(int id)
    {
        return _events.FirstOrDefault(x => x.Id == id);
    }

    /// <inheritdoc />
    public ReadOnlyCollection<Event> GetEvents()
    {
        return _events.AsReadOnly();
    }

    /// <inheritdoc />
    public int AddEvent(Event eventToAdd)
    {
        eventToAdd.Id = _nextId++;
        _events.Add(eventToAdd);

        return eventToAdd.Id;
    }

    /// <inheritdoc />
    public void UpdateEvent(Event updatedEvent)
    {
        _events[updatedEvent.Id] = updatedEvent;
    }

    /// <inheritdoc />
    public bool RemoveEvent(Event eventToRemove)
    {
        return _events.Remove(eventToRemove);
    }
}
