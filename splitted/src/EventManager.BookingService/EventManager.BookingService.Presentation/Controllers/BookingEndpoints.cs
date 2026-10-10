using Asp.Versioning;

using EventManager.BookingService.Application.Abstractions.Services;
using EventManager.BookingService.Application.Common.Results;
using EventManager.BookingService.Presentation.Models.Response;
using EventManager.Contracts.Jwt;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace EventManager.BookingService.Presentation.Controllers;

[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "v1")]
[Route("api/v{version:apiVersion}")]
public class BookingEndpoints : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<BookingEndpoints> _logger;

    public BookingEndpoints(IBookingService bookingService, ILogger<BookingEndpoints> logger)
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
    [HttpPost("events/{id:int}/book")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> BookEventAsync([FromRoute] int id, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub);
        var roleClaim = User.FindFirst(JwtRoles.RoleClaimName);
        
        if (userIdClaim == null || roleClaim == null)
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

        return AcceptedAtAction(nameof(GetBookingById),new { id = newBooking.Id }, bookingResponse);
    }

    /// <summary>
    /// Получение бронирования по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор бронирования</param>
    /// <param name="ct">Токен отмены</param>
    /// <response code="200">Возвращается бронирование</response>
    /// <response code="404">Бронирование не найдено</response>
    /// <response code="401">Пользователь не авторизован</response>
    [HttpGet("bookings/{id:int}")]
    [ProducesResponseType(typeof(BookingInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BookingInfoResponse>> GetBookingById(int id, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub);
        var roleClaim = User.FindFirst(JwtRoles.RoleClaimName);
        
        if (userIdClaim == null || roleClaim == null)
            return Problem(detail: "Invalid token.", statusCode: StatusCodes.Status401Unauthorized);

        if (!int.TryParse(userIdClaim.Value, out var userId))
            return Problem(detail: "Invalid token.", statusCode: StatusCodes.Status401Unauthorized);

        var result = await _bookingService.GetBookingByIdAsync(id, userId, roleClaim.Value, ct);

        if (!result.IsSuccess)
        {
            return Problem(detail: result.Error!.ErrorMessage, statusCode: result.Error!.GetHttpStatusCodeForError());
        }

        var response = new BookingInfoResponse
        {
            Id = result.Value!.Id,
            EventId = result.Value.EventId,
            Status = result.Value.Status,
            CreatedAt = result.Value.CreatedAt,
            ProcessedAt = result.Value.ProcessedAt
        };

        return Ok(response);
    }

    /// <summary>
    /// Отмена бронирования
    /// </summary>
    /// <param name="id">Идентификатор бронирования</param>
    /// <param name="ct">Токен отмены</param>
    /// <response code="204">Бронирование отменено</response>
    /// <response code="404">Бронирование/пользователь не найден</response>
    /// <response code="401">Пользователь не авторизован</response>
    [HttpDelete("bookings/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelBookingAsync(int id, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub);
        var roleClaim = User.FindFirst(JwtRoles.RoleClaimName);
        
        if (userIdClaim == null || roleClaim == null)
            return Problem(detail: "Invalid token.", statusCode: StatusCodes.Status401Unauthorized);

        if (!int.TryParse(userIdClaim.Value, out var userId))
            return Problem(detail: "Invalid token.", statusCode: StatusCodes.Status401Unauthorized);

        var result = await _bookingService.CancelBookingAsync(id, userId, roleClaim.Value, ct);
        if (!result.IsSuccess)
        {
            return Problem(detail: result.Error!.ErrorMessage, statusCode: result.Error!.GetHttpStatusCodeForError());
        }

        return NoContent();
    }
}