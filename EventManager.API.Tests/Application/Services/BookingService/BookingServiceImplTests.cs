using EventManager.API.Application.Services.BookingService;
using EventManager.API.Application.Services.EventService;
using EventManager.API.Domain.Interfaces;
using EventManager.API.Models.Entities;
using EventManager.API.Models.Results;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

namespace EventManager.API.Tests.Application.Services.BookingService;

public class BookingServiceImplTests
{
    private readonly IBookingService _bookingService;
    private readonly Mock<IBookingRepository> _bookingRepositoryMock;
    private readonly Mock<IEventService> _eventServiceMock;

    public BookingServiceImplTests()
    {
        _bookingRepositoryMock = new Mock<IBookingRepository>();
        _eventServiceMock = new Mock<IEventService>();
        _bookingService = new BookingServiceImpl(_bookingRepositoryMock.Object, _eventServiceMock.Object,
            NullLogger<BookingServiceImpl>.Instance);
    }

    [Fact]
    public async Task CreateBookingAsync_WhenEventExists_ShouldReturnNewBookingId()
    {
        const int eventId = 1;
        const int expectedBookingId = 10;
        _eventServiceMock.Setup(x => x.GetEventById(eventId)).Returns(Result<Event?>.Success(new Event
        {
            Id = eventId, Title = "Test Event", StartAt = DateTime.Now, EndAt = DateTime.Now
        }));
        _bookingRepositoryMock.Setup(x => x.AddBooking(It.Is<Booking>(b => b.EventId == eventId)))
            .Returns(expectedBookingId);

        var bookingResult = await _bookingService.CreateBookingAsync(eventId, TestContext.Current.CancellationToken);

        bookingResult.IsSuccess.Should().BeTrue();
        var actualBookingId = bookingResult.Value!.Id;
        actualBookingId.Should().Be(expectedBookingId);
        _bookingRepositoryMock.Verify(x => x.AddBooking(It.Is<Booking>(b => b.EventId == eventId)), Times.Once);
    }

    [Fact]
    public async Task CreateBookingAsync_WhenEventDoesNotExist_ShouldReturnNotFoundError()
    {
        const int eventId = 1;
        _eventServiceMock.Setup(x => x.GetEventById(eventId)).Returns(Result<Event?>.Failure(Error.NotFound("")));

        var bookingResult = await _bookingService.CreateBookingAsync(eventId, TestContext.Current.CancellationToken);

        bookingResult.IsSuccess.Should().BeFalse();
        bookingResult.Error!.ErrorType.Should().Be(ErrorType.NotFound);
        _bookingRepositoryMock.Verify(x => x.AddBooking(It.IsAny<Booking>()), Times.Never);
    }

    [Fact]
    public async Task CreateBookingAsync_WhenEventHasBookings_ShouldReturnUniqueBookingId()
    {
        const int eventId = 1;
        const int expectedBookingId1 = 10;
        const int expectedBookingId2 = 11;
        _eventServiceMock.Setup(x => x.GetEventById(eventId)).Returns(Result<Event?>.Success(new Event
        {
            Id = eventId, Title = "Test Event", StartAt = DateTime.Now, EndAt = DateTime.Now
        }));
        _bookingRepositoryMock.SetupSequence(x => x.AddBooking(It.Is<Booking>(b => b.EventId == eventId)))
            .Returns(expectedBookingId1)
            .Returns(expectedBookingId2);

        var bookingResult1 = await _bookingService.CreateBookingAsync(eventId, TestContext.Current.CancellationToken);
        var bookingResult2 = await _bookingService.CreateBookingAsync(eventId, TestContext.Current.CancellationToken);

        bookingResult1.IsSuccess.Should().BeTrue();
        bookingResult2.IsSuccess.Should().BeTrue();
        var actualBookingId1 = bookingResult1.Value!.Id;
        var actualBookingId2 = bookingResult2.Value!.Id;
        actualBookingId1.Should().Be(expectedBookingId1);
        actualBookingId2.Should().Be(expectedBookingId2);
        _bookingRepositoryMock.Verify(x => x.AddBooking(It.Is<Booking>(b => b.EventId == eventId)), Times.Exactly(2));
    }

    [Fact]
    public async Task GetBookingByIdAsync_WhenBookingExists_ShouldReturnValidBooking()
    {
        const int bookingId = 1;
        var expectedBooking = new Booking
        {
            Id = bookingId,
            EventId = 1,
            Status = BookingStatus.Confirmed,
            CreatedAt = new DateTime(2026, 1, 1),
            ProcessedAt = new DateTime(2026, 1, 2),
        };

        _bookingRepositoryMock.Setup(x => x.GetBookingById(bookingId)).Returns(expectedBooking);

        var result = await _bookingService.GetBookingByIdAsync(bookingId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedBooking);
    }

    [Fact]
    public async Task GetBookingByIdAsync_WhenBookingStatusChanged_ShouldReturnUpdatedBookingStatus()
    {
        const int bookingId = 1;
        var expectedBooking = new Booking
        {
            Id = bookingId,
            EventId = 1,
            Status = BookingStatus.Pending,
            CreatedAt = new DateTime(2026, 1, 1),
            ProcessedAt = new DateTime(2026, 1, 2),
        };

        _bookingRepositoryMock.Setup(x => x.GetBookingById(bookingId)).Returns(expectedBooking);

        var resultBefore = await _bookingService.GetBookingByIdAsync(bookingId, TestContext.Current.CancellationToken);
        resultBefore.IsSuccess.Should().BeTrue();
        resultBefore.Value!.Status.Should().Be(BookingStatus.Pending);

        expectedBooking.Status = BookingStatus.Confirmed;

        var resultAfter = await _bookingService.GetBookingByIdAsync(bookingId, TestContext.Current.CancellationToken);
        resultAfter.IsSuccess.Should().BeTrue();
        resultAfter.Value!.Status.Should().Be(BookingStatus.Confirmed);
    }

    [Fact]
    public async Task GetBookingByIdAsync_WhenBookingDoesNotExist_ShouldReturnNotFoundError()
    {
        const int bookingId = 1;
        _bookingRepositoryMock.Setup(x => x.GetBookingById(bookingId)).Returns((Booking?)null);

        var result = await _bookingService.GetBookingByIdAsync(bookingId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error!.ErrorType.Should().Be(ErrorType.NotFound);
    }
}