using System.Collections.ObjectModel;

using EventManager.API.Application.Interfaces;
using EventManager.API.Domain.Interfaces;
using EventManager.API.Models;

namespace EventManager.API.Application.Services;

/// <summary>
/// Сервис для работы с мероприятиями
/// </summary>
public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;

    public EventService(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    /// <inheritdoc />
    public int AddEvent(Event eventToAdd)
    {
        var newId = _eventRepository.AddEvent(eventToAdd);

        return newId;
    }

    /// <inheritdoc />
    public ReadOnlyCollection<Event> GetEvents()
    {
        return _eventRepository.GetEvents();
    }

    /// <inheritdoc />
    public Event? GetEventById(int id)
    {
        return _eventRepository.GetEventById(id);
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
}
