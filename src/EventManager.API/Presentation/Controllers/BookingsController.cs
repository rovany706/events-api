using System.Globalization;

using Asp.Versioning;

using EventManager.API.Application.Services.BookingService;
using EventManager.API.Models.Response;
using EventManager.API.Models.Results;

using Microsoft.AspNetCore.Mvc;

namespace EventManager.API.Presentation.Controllers;

[ApiController]
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
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingResponse>> GetBookingById(int id, CancellationToken ct)
    {
        var result = await _bookingService.GetBookingByIdAsync(id, ct);

        if (!result.IsSuccess && result.Error!.ErrorType == ErrorType.NotFound)
        {
            return Problem(detail: string.Format(CultureInfo.InvariantCulture, Resource.ErrorBookingNotFound, id),
                statusCode: StatusCodes.Status404NotFound);
        }

        return Ok(result.Value);
    }
}