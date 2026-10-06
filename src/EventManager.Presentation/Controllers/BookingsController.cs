using System.Globalization;

using Asp.Versioning;

using EventManager.Application.Abstractions.Services;
using EventManager.Application.Common.Results;
using EventManager.Presentation.Models.Response;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace EventManager.Presentation.Controllers;

[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "v1")]
[Route("api/v{version:apiVersion}/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(IBookingService bookingService, ILogger<BookingsController> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    /// <summary>
    /// Получение бронирования по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор бронирования</param>
    /// <param name="ct">Токен отмены</param>
    /// <response code="200">Возвращается бронирование</response>
    /// <response code="404">Бронирование не найдено</response>
    /// <response code="401">Пользователь не авторизован</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BookingInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BookingInfoResponse>> GetBookingById(int id, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub);

        if (userIdClaim == null)
            return Problem(detail: "Invalid token.", statusCode: StatusCodes.Status401Unauthorized);

        if (!int.TryParse(userIdClaim.Value, out var userId))
            return Problem(detail: "Invalid token.", statusCode: StatusCodes.Status401Unauthorized);

        var result = await _bookingService.GetBookingByIdAsync(id, userId, ct);

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
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelBookingAsync(int id, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub);

        if (userIdClaim == null)
            return Problem(detail: "Invalid token.", statusCode: StatusCodes.Status401Unauthorized);

        if (!int.TryParse(userIdClaim.Value, out var userId))
            return Problem(detail: "Invalid token.", statusCode: StatusCodes.Status401Unauthorized);

        var result = await _bookingService.CancelBookingAsync(id, userId, ct);
        if (!result.IsSuccess)
        {
            return Problem(detail: result.Error!.ErrorMessage, statusCode: result.Error!.GetHttpStatusCodeForError());
        }

        return NoContent();
    }
}