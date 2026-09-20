using EventManager.API.Application.Services.BookingService;
using EventManager.API.Application.Services.EventService;
using EventManager.API.Domain.DataAccess;
using EventManager.API.Domain.Repositories;
using EventManager.API.Models.Entities;
using EventManager.API.Models.Request;
using EventManager.API.Models.Results;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EventManager.API.Tests.Application.Services.BookingService;

public class BookingServiceImplTests
{
    private readonly IBookingService _bookingService;
    private readonly Mock<IBookingRepository> _bookingRepositoryMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;

    public BookingServiceImplTests()
    {
        _eventRepositoryMock = new Mock<IEventRepository>();
        _bookingRepositoryMock = new Mock<IBookingRepository>();
        _bookingService = new BookingServiceImpl(_eventRepositoryMock.Object, _bookingRepositoryMock.Object,
            NullLogger<BookingServiceImpl>.Instance);
    }

    private async Task<int> CreateTestEvent(int totalSeats = 20)
    {
        var id = await _eventService.AddEvent(
            new CreateEventRequest
            {
                Title = "Test Event",
                Description = null,
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(1),
                TotalSeats = totalSeats
            }, TestContext.Current.CancellationToken);

        return id;
    }

    [Fact]
    public async Task CreateBookingAsync_WhenEventExists_ShouldReturnNewBookingId()
    {
        var eventId = await CreateTestEvent();

        var bookingResult = await _bookingService.CreateBookingAsync(eventId, TestContext.Current.CancellationToken);

        bookingResult.IsSuccess.Should().BeTrue();
        var actualBookingId = bookingResult.Value!.Id;
        actualBookingId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateBookingAsync_WhenEventDoesNotExist_ShouldReturnNotFoundError()
    {
        const int eventId = 1;

        var bookingResult = await _bookingService.CreateBookingAsync(eventId, TestContext.Current.CancellationToken);

        bookingResult.IsSuccess.Should().BeFalse();
        bookingResult.Error!.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task CreateBookingAsync_WhenEventHasBookings_ShouldReturnUniqueBookingId()
    {
        const int expectedBookingId1 = 1;
        const int expectedBookingId2 = 2;
        var eventId = await CreateTestEvent();

        var bookingResult1 = await _bookingService.CreateBookingAsync(eventId, TestContext.Current.CancellationToken);
        var bookingResult2 = await _bookingService.CreateBookingAsync(eventId, TestContext.Current.CancellationToken);

        bookingResult1.IsSuccess.Should().BeTrue();
        bookingResult2.IsSuccess.Should().BeTrue();
        var actualBookingId1 = bookingResult1.Value!.Id;
        var actualBookingId2 = bookingResult2.Value!.Id;
        actualBookingId1.Should().Be(expectedBookingId1);
        actualBookingId2.Should().Be(expectedBookingId2);
    }

    [Fact]
    public async Task GetBookingByIdAsync_WhenBookingExists_ShouldReturnValidBooking()
    {
        var eventId = await CreateTestEvent();
        var createdBooking = (await _bookingService.CreateBookingAsync(eventId, TestContext.Current.CancellationToken))
            .Value!;

        var result =
            await _bookingService.GetBookingByIdAsync(createdBooking.Id, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(createdBooking.Id);
        result.Value.EventId.Should().Be(createdBooking.EventId);
        result.Value.Status.Should().Be(BookingStatus.Pending);
    }

    [Fact]
    public async Task GetBookingByIdAsync_WhenBookingDoesNotExist_ShouldReturnNotFoundError()
    {
        const int bookingId = 1;

        var result = await _bookingService.GetBookingByIdAsync(bookingId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error!.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task CreateBookingAsync_Always_ShouldDecreaseEventAvailableSeats()
    {
        const int expectedAvailableSeats = 2;
        var eventId = await CreateTestEvent(expectedAvailableSeats + 1);

        var bookingResult = await _bookingService.CreateBookingAsync(eventId, TestContext.Current.CancellationToken);

        var updatedEventInfo =
            (await _eventService.GetEventById(eventId, TestContext.Current.CancellationToken)).Value!;
        bookingResult.IsSuccess.Should().BeTrue();
        updatedEventInfo.AvailableSeats.Should().Be(expectedAvailableSeats);
    }

    [Fact]
    public async Task CreateBookingAsync_WhenNoSeatsAvailable_ShouldReturnConflictError()
    {
        var eventId = await CreateTestEvent(1);

        var successfulBook = await _bookingService.CreateBookingAsync(eventId, TestContext.Current.CancellationToken);
        var unsuccessfulBook = await _bookingService.CreateBookingAsync(eventId, TestContext.Current.CancellationToken);

        var updatedEventInfo =
            (await _eventService.GetEventById(eventId, TestContext.Current.CancellationToken)).Value!;
        updatedEventInfo.AvailableSeats.Should().Be(0);
        successfulBook.IsSuccess.Should().BeTrue();
        unsuccessfulBook.IsSuccess.Should().BeFalse();
        unsuccessfulBook.Error!.ErrorType.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task CreateBookingAsync_WhenConcurrent_ShouldReturnUniqueIds()
    {
        const int totalSeats = 10;
        var eventId = await CreateTestEvent(totalSeats);

        var tasks = new Task<Result<Booking?>>[totalSeats];

        for (var i = 0; i < totalSeats; i++)
        {
            tasks[i] = Task.Run(
                async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    return await bookingService.CreateBookingAsync(eventId, TestContext.Current.CancellationToken);
                }, TestContext.Current.CancellationToken);
        }

        await Task.WhenAll(tasks);

        var bookingResults = tasks.Select(x => x.Result).ToList();

        bookingResults.All(x => x.IsSuccess).Should().BeTrue();
        bookingResults.Select(x => x.Value!.Id).Should().BeEquivalentTo(Enumerable.Range(1, totalSeats));
    }
}