using EventManager.API.Application.Services.BookingService;
using EventManager.API.Application.Services.EventService;
using EventManager.API.Models.Entities;
using EventManager.API.Models.Results;
using EventManager.API.Presentation.Controllers;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

namespace EventManager.API.Tests.Presentation.Controllers;

public class EventsControllerTests
{
    private readonly Mock<IEventService> _eventServiceMock;
    private readonly Mock<IBookingService> _bookingServiceMock;
    private readonly EventsController _controller;

    public EventsControllerTests()
    {
        _eventServiceMock = new Mock<IEventService>();
        _bookingServiceMock = new Mock<IBookingService>();
        _controller = new EventsController(_eventServiceMock.Object, _bookingServiceMock.Object, NullLogger<EventsController>.Instance);
    }

    [Fact]
    public async Task BookEventAsync_WhenBookingCreated_ShouldReturn202AcceptedAt()
    {
        const int id = 1;
        var booking = new Booking
        {
            Id = 1,
            CreatedAt = DateTime.UtcNow,
            EventId = id,
            Status = BookingStatus.Pending
        };
        _bookingServiceMock.Setup(x => x.CreateBookingAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(Result<Booking?>.Success(booking));

        var actual = await _controller.BookEventAsync(id, TestContext.Current.CancellationToken);
        var result = Assert.IsType<AcceptedAtActionResult>(actual);

        result.StatusCode.Should().Be(StatusCodes.Status202Accepted);
        result.ActionName.Should().Be(nameof(BookingsController.GetBookingById));
        result.ControllerName.Should().Be("Bookings");
        result.RouteValues.Should().ContainKey("id");
    }

    [Fact]
    public async Task BookEventAsync_WhenCreateBookingReturnsConflictError_ShouldReturn409Conflict()
    {
        const int id = 1;
        const string expectedErrorMessage = "Conflict";
        _bookingServiceMock.Setup(x => x.CreateBookingAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Booking?>.Failure(Error.Conflict(expectedErrorMessage)));

        var actual = await _controller.BookEventAsync(id, TestContext.Current.CancellationToken);
        var result = Assert.IsType<ObjectResult>(actual);
        var body = Assert.IsType<ProblemDetails>(result.Value);

        result.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        body.Detail.Should().Be(expectedErrorMessage);
    }

    [Fact]
    public async Task BookEventAsync_WhenCreateBookingReturnsNotFoundError_ShouldReturn404NotFound()
    {
        const int id = 1;
        const string expectedErrorMessage = "Мероприятие с id ? не найдено.";
        _bookingServiceMock.Setup(x => x.CreateBookingAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Booking?>.Failure(Error.NotFound("")));

        var actual = await _controller.BookEventAsync(id, TestContext.Current.CancellationToken);
        var result = Assert.IsType<ObjectResult>(actual);
        var body = Assert.IsType<ProblemDetails>(result.Value);

        result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        body.Detail.Should().Match(expectedErrorMessage);
    }
}
