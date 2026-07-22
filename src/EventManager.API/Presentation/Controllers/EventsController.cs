using System.Globalization;

using Asp.Versioning;

using EventManager.API.Application.Services.BookingService;
using EventManager.API.Application.Services.EventService;
using EventManager.API.Application.Services.EventService.Models;
using EventManager.API.Models.Mapping;
using EventManager.API.Models.Request;
using EventManager.API.Models.Response;
using EventManager.API.Models.Results;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EventManager.API.Presentation.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "v1")]
[Route("api/v{version:apiVersion}/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly IBookingService _bookingService;
    private readonly ILogger<EventsController> _logger;

    public EventsController(IEventService eventService, IBookingService bookingService,
        ILogger<EventsController> logger)
    {
        _eventService = eventService;
        _bookingService = bookingService;
        _logger = logger;
    }

    /// <summary>
    /// Получение всех мероприятий
    /// </summary>
    /// <response code="200">Возвращается список мероприятий</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<EventInfoResponse>), StatusCodes.Status200OK)]
    public ActionResult<PaginatedResult<EventInfoResponse>> GetAllEvents([FromQuery] GetEventsFilterParams filters,
        [FromQuery] PaginationParams paginationParams)
    {
        _logger.LogDebug("Получен запрос на получение всех мероприятий");
        _logger.LogDebug("Filters: {0}", filters);
        _logger.LogDebug("Pagination params: {0}", paginationParams);

        var filterDto = new EventFilterDto { Title = filters.Title, From = filters.From, To = filters.To };

        var events = _eventService.GetEvents(filterDto, paginationParams);
        return Ok(new PaginatedResult<EventInfoResponse>(
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
    [ProducesResponseType(typeof(EventInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<EventInfoResponse> GetEventById(int id)
    {
        _logger.LogDebug("Получен запрос на получение мероприятия по идентификатору (id = {Id})", id);

        var result = _eventService.GetEventById(id);

        if (!result.IsSuccess)
        {
            return GetProblem(result.Error!, id);
        }

        var eventToSend = result.Value!;

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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult UpdateEvent(int id, [FromBody] UpdateEventRequest updateEventRequest)
    {
        _logger.LogDebug("Получен запрос на обновление информации о мероприятии (id = {Id})", id);

        var updateResult = _eventService.TryUpdateEvent(updateEventRequest.ToEvent(id));

        if (!updateResult)
        {
            return Problem(detail: GetEventNotFoundErrorMessage(id), statusCode: StatusCodes.Status404NotFound);
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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult DeleteEvent(int id)
    {
        _logger.LogDebug("Получен запрос на удаление мероприятия (id = {Id})", id);

        var removeResult = _eventService.TryRemoveEvent(id);

        if (!removeResult)
        {
            return Problem(detail: GetEventNotFoundErrorMessage(id), statusCode: StatusCodes.Status404NotFound);
        }

        return NoContent();
    }

    /// <summary>
    /// Бронирование мероприятия
    /// </summary>
    /// <param name="id">Идентификатор мероприятия</param>
    /// <param name="ct">Токен отмены</param>
    /// <response code="202">Бронь зарегистрирована</response>
    /// <response code="404">Мероприятие не найдено</response>
    /// <response code="409">Мест для бронирования нет</response>
    [HttpPost("{id:int}/book")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> BookEventAsync([FromRoute] int id, CancellationToken ct)
    {
        var result = await _bookingService.CreateBookingAsync(id, ct);

        if (!result.IsSuccess)
        {
            return GetProblem(result.Error!, id);
        }

        var newBooking = result.Value!;

        return AcceptedAtAction(nameof(BookingsController.GetBookingById), "Bookings",
            new { id = newBooking.Id }, newBooking);
    }

    private ObjectResult GetProblem(Error error, params object[] errorMessageFormatParams)
    {
        return error.ErrorType switch
        {
            ErrorType.NotFound => Problem(detail: GetEventNotFoundErrorMessage(errorMessageFormatParams[0]), statusCode: StatusCodes.Status404NotFound),
            ErrorType.Conflict => Problem(detail: error.ErrorMessage, statusCode: StatusCodes.Status409Conflict),
            _ => throw new Exception($"Unknown error {error.ErrorType}. Message: {error.ErrorMessage}")
        };
    }

    private static string GetEventNotFoundErrorMessage(object id)
    {
        return string.Format(CultureInfo.InvariantCulture, Resource.ErrorEventNotFound, id);
    }
}