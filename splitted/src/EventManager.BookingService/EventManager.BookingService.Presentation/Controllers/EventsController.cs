using System.IdentityModel.Tokens.Jwt;

using Asp.Versioning;

using EventManager.BookingService.Application.Abstractions.Services;
using EventManager.BookingService.Application.Common.Results;
using EventManager.BookingService.Presentation.Models.Response;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManager.BookingService.Presentation.Controllers;

/// <summary>
/// Контроллер для работы с мероприятиями
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "v1")]
[Route("api/v{version:apiVersion}/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<EventsController> _logger;
    
    public EventsController(IBookingService bookingService,
        ILogger<EventsController> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }
    
    /// <summary>
    /// Бронирование мероприятия
    /// </summary>
    /// <param name="id">Идентификатор мероприятия</param>
    /// <param name="ct">Токен отмены</param>
    /// <response code="202">Бронь зарегистрирована</response>
    /// <response code="404">Мероприятие не найдено</response>
    /// <response code="409">Мест для бронирования нет</response>
    /// <response code="401">Пользователь не авторизован</response>
    [Authorize]
    [HttpPost("{id:int}/book")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> BookEventAsync([FromRoute] int id, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub);

        if (userIdClaim == null)
            return Problem(detail: "Invalid token.", statusCode: StatusCodes.Status401Unauthorized);

        if (!int.TryParse(userIdClaim.Value, out var userId))
            return Problem(detail: "Invalid token.", statusCode: StatusCodes.Status401Unauthorized);

        var result = await _bookingService.CreateBookingAsync(id, userId, ct);

        if (!result.IsSuccess)
        {
            return Problem(detail: result.Error!.ErrorMessage, statusCode: result.Error!.GetHttpStatusCodeForError());
        }

        var newBooking = result.Value!;
        var bookingResponse = new BookingInfoResponse
        {
            Id = newBooking.Id,
            EventId = newBooking.EventId,
            Status = newBooking.Status,
            CreatedAt = newBooking.CreatedAt,
            ProcessedAt = newBooking.ProcessedAt
        };

        return AcceptedAtAction(nameof(BookingsController.GetBookingById), "Bookings",
            new { id = newBooking.Id }, bookingResponse);
    }
}