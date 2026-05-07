using System.Globalization;

using EventManager.API.Application.Interfaces;
using EventManager.API.Models;

using Microsoft.AspNetCore.Mvc;

namespace EventManager.API.Presentation.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "v1")]
[Route("api/v{version:apiVersion}/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly ILogger<EventsController> _logger;

    public EventsController(IEventService eventService, ILogger<EventsController> logger)
    {
        _eventService = eventService;
        _logger = logger;
    }

    /// <summary>
    /// Получение всех мероприятий
    /// </summary>
    /// <response code="200">Возвращается список мероприятий</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EventDto>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<EventDto>> GetAllEvents()
    {
        _logger.LogDebug("Получен запрос на получение всех мероприятий");

        var events = _eventService.GetAllEvents();

        return Ok(events);
    }

    /// <summary>
    /// Получение мероприятия по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор мероприятия</param>
    /// <response code="200">Возвращается мероприятие</response>
    /// <response code="404">Мероприятие не найдено</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<EventDto> GetEventById(int id)
    {
        _logger.LogDebug("Получен запрос на получение мероприятия по идентификатору");

        var eventToSend = _eventService.GetEventById(id);

        if (eventToSend == null)
        {
            return NotFound(GetEventNotFoundErrorMessage(id));
        }

        return Ok(eventToSend);
    }

    /// <summary>
    /// Создание мероприятия
    /// </summary>
    /// <param name="eventDto">Мероприятие</param>
    /// <response code="201">Мероприятие создано</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public IActionResult CreateEvent(EventDto eventDto)
    {
        _logger.LogDebug("Получен запрос на создание мероприятия");

        var eventId = _eventService.AddEvent(eventDto);

        return CreatedAtAction(nameof(GetEventById), new { id = eventId }, eventDto);
    }

    /// <summary>
    /// Обновление информации о мероприятии
    /// </summary>
    /// <param name="eventDto">Мероприятие</param>
    /// <response code="204">Мероприятие обновлено</response>
    /// <response code="404">Мероприятие не найдено</response>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdateEvent(EventDto eventDto)
    {
        _logger.LogDebug("Получен запрос на обновление информации о мероприятии");

        var updateResult = _eventService.TryUpdateEvent(eventDto);

        if (updateResult == false)
        {
            return NotFound(GetEventNotFoundErrorMessage(eventDto.Id));
        }

        return NoContent();
    }

    /// <summary>
    /// Удаление мероприятия
    /// </summary>
    /// <param name="id">Идентификатор мероприятия</param>
    /// <response code="204">Мероприятие удалено</response>
    /// <response code="404">Мероприятие не найдено</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteEvent(int id)
    {
        _logger.LogDebug("Получен запрос на удаление мероприятия");

        var removeResult = _eventService.TryRemoveEvent(id);

        if (removeResult == false)
        {
            return NotFound(GetEventNotFoundErrorMessage(id));
        }

        return NoContent();
    }

    private static string GetEventNotFoundErrorMessage(int id)
    {
        return string.Format(CultureInfo.InvariantCulture, Resource.ErrorEventNotFound, id);
    }
}
