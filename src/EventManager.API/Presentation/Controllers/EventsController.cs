using System.Globalization;

using Asp.Versioning;

using EventManager.API.Application.Services.EventService;
using EventManager.API.Application.Services.EventService.Models;
using EventManager.API.Models.Mapping;
using EventManager.API.Models.Request;
using EventManager.API.Models.Response;

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
    [ProducesResponseType(typeof(PaginatedResult<EventResponse>), StatusCodes.Status200OK)]
    public ActionResult<PaginatedResult<EventResponse>> GetAllEvents([FromQuery] GetEventsFilterParams filters, [FromQuery] PaginatonParams paginatonParams)
    {
        _logger.LogDebug("Получен запрос на получение всех мероприятий");
        _logger.LogDebug("Filters: {0}", filters);
        _logger.LogDebug("Pagination params: {0}", paginatonParams);

        var filterDto = new EventFilterDto
        {
            Title = filters.Title,
            From = filters.From,
            To = filters.To
        };

        var events = _eventService.GetEvents(filterDto, paginatonParams);
        return Ok(new PaginatedResult<EventResponse>(
            events.Items.Select(x => x.ToEventResponse()).ToList(),
            events.ItemCount,
            events.CurrentPage,
            events.TotalPages,
            events.TotalItems
        ));
    }

    /// <summary>
    /// Получение мероприятия по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор мероприятия</param>
    /// <response code="200">Возвращается мероприятие</response>
    /// <response code="404">Мероприятие не найдено</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<EventResponse> GetEventById(int id)
    {
        _logger.LogDebug("Получен запрос на получение мероприятия по идентификатору (id = {Id})", id);

        var eventToSend = _eventService.GetEventById(id);

        if (eventToSend == null)
        {
            return NotFound(GetEventNotFoundErrorMessage(id));
        }

        return Ok(eventToSend.ToEventResponse());
    }

    /// <summary>
    /// Создание мероприятия
    /// </summary>
    /// <param name="createEventRequest">Запрос на создание мероприятия</param>
    /// <response code="201">Мероприятие создано</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public IActionResult CreateEvent(CreateEventRequest createEventRequest)
    {
        _logger.LogDebug("Получен запрос на создание мероприятия");

        var eventId = _eventService.AddEvent(createEventRequest.ToEvent());

        return CreatedAtAction(nameof(GetEventById), new { id = eventId }, createEventRequest);
    }

    /// <summary>
    /// Обновление информации о мероприятии
    /// </summary>
    /// <param name="id">Идентификатор мероприятия</param>
    /// <param name="updateEventRequest">Запрос на обновление мероприятия</param>
    /// <response code="204">Мероприятие обновлено</response>
    /// <response code="404">Мероприятие не найдено</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdateEvent(int id, [FromBody] UpdateEventRequest updateEventRequest)
    {
        _logger.LogDebug("Получен запрос на обновление информации о мероприятии (id = {Id})", id);

        var updateResult = _eventService.TryUpdateEvent(updateEventRequest.ToEvent(id));

        if (updateResult == false)
        {
            return NotFound(GetEventNotFoundErrorMessage(id));
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
        _logger.LogDebug("Получен запрос на удаление мероприятия (id = {Id})", id);

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
