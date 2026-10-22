using Asp.Versioning;

using EventManager.EventService.Application.Abstractions.Services;
using EventManager.EventService.Application.Abstractions.Services.Dto;
using EventManager.EventService.Application.Common.Pagination;
using EventManager.EventService.Application.Common.Results;
using EventManager.EventService.Presentation.Models.Request;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManager.EventService.Presentation.Controllers;

/// <summary>
/// Контроллер для работы с мероприятиями
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "v1")]
[Route("api/v{version:apiVersion}/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly ILogger<EventsController> _logger;

    public EventsController(IEventService eventService,
        ILogger<EventsController> logger)
    {
        _eventService = eventService;
        _logger = logger;
    }

    /// <summary>
    /// Получение всех мероприятий
    /// </summary>
    /// <response code="200">Возвращается список мероприятий</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<EventInfoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<EventInfoDto>>> GetAllEvents(
        [FromQuery] GetEventsFilterParams filters,
        [FromQuery] PaginationParams paginationParams, CancellationToken ct)
    {
        _logger.LogDebug("Получен запрос на получение всех мероприятий");
        _logger.LogDebug("Filters: {0}", filters);
        _logger.LogDebug("Pagination params: {0}", paginationParams);

        var filterDto = new EventFilterDto { Title = filters.Title, From = filters.From, To = filters.To };
        var paginationDto = new PaginationParamsDto(paginationParams.Page, paginationParams.PageSize);
        var events = await _eventService.GetEvents(filterDto, paginationDto, ct);

        return Ok(new PaginatedResult<EventInfoDto>(
            events.Items,
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
    /// <param name="ct">Токен отмены</param>
    /// <response code="200">Возвращается мероприятие</response>
    /// <response code="404">Мероприятие не найдено</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EventInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventInfoDto>> GetEventById(int id, CancellationToken ct)
    {
        _logger.LogDebug("Получен запрос на получение мероприятия по идентификатору (id = {Id})", id);

        var result = await _eventService.GetEventById(id, ct);

        if (!result.IsSuccess)
        {
            return Problem(detail: result.Error!.ErrorMessage, statusCode: result.Error!.GetHttpStatusCodeForError());
        }

        var eventToSend = result.Value!;

        return Ok(eventToSend);
    }

    /// <summary>
    /// Создание мероприятия
    /// </summary>
    /// <param name="createEventRequest">Запрос на создание мероприятия</param>
    /// <param name="ct">Токен отмены</param>
    /// <response code="201">Мероприятие создано</response>
    /// <response code="401">Пользователь не авторизован</response>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateEvent(CreateEventRequest createEventRequest, CancellationToken ct)
    {
        _logger.LogDebug("Получен запрос на создание мероприятия");

        var eventId = await _eventService.AddEvent(createEventRequest, ct);
        var response =
            new EventInfoDto(eventId, createEventRequest.Title, createEventRequest.Description,
                createEventRequest.StartAt, createEventRequest.EndAt, createEventRequest.TotalSeats,
                createEventRequest.TotalSeats);

        return CreatedAtAction(nameof(GetEventById), new { id = eventId }, response);
    }

    /// <summary>
    /// Обновление информации о мероприятии
    /// </summary>
    /// <param name="id">Идентификатор мероприятия</param>
    /// <param name="updateEventRequest">Запрос на обновление мероприятия</param>
    /// <param name="ct">Токен отмены</param>
    /// <response code="204">Мероприятие обновлено</response>
    /// <response code="404">Мероприятие не найдено</response>
    /// <response code="401">Пользователь не авторизован</response>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventRequest updateEventRequest,
        CancellationToken ct)
    {
        _logger.LogDebug("Получен запрос на обновление информации о мероприятии (id = {Id})", id);

        var result = await _eventService.UpdateEvent(id, updateEventRequest, ct);

        if (!result.IsSuccess)
        {
            return Problem(detail: result.Error!.ErrorMessage, statusCode: result.Error!.GetHttpStatusCodeForError());
        }

        return NoContent();
    }

    /// <summary>
    /// Удаление мероприятия
    /// </summary>
    /// <param name="id">Идентификатор мероприятия</param>
    /// <param name="ct">Токен отмены</param>
    /// <response code="204">Мероприятие удалено</response>
    /// <response code="404">Мероприятие не найдено</response>
    /// <response code="401">Пользователь не авторизован</response>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteEvent(int id, CancellationToken ct)
    {
        _logger.LogDebug("Получен запрос на удаление мероприятия (id = {Id})", id);

        var result = await _eventService.RemoveEvent(id, ct);

        if (!result.IsSuccess)
        {
            return Problem(detail: result.Error!.ErrorMessage, statusCode: result.Error!.GetHttpStatusCodeForError());
        }

        return NoContent();
    }

    /// <summary>
    /// Получение топ-10 популярных мероприятий
    /// </summary>
    /// <param name="ct">Токен отмены</param>
    /// <response code="200">Список из 10 мероприятий, отсортированный по популярности</response>
    [HttpGet("top")]
    [ProducesResponseType(typeof(List<EventInfoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EventInfoDto>>> GetTop10Events(CancellationToken ct)
    {
        var top10 = await _eventService.GetTop10PopularEvents(ct);

        return Ok(top10);
    }
}