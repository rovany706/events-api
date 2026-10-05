using EventManager.Application.Abstractions.Persistence.Repositories;
using EventManager.Application.Common.Results;
using EventManager.Application.Services.BookingService;
using EventManager.Domain.Entities;
using EventManager.Domain.Entities.Bookings;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

namespace EventManager.Application.Tests.Services.BookingService;

public class BookingServiceImplTests
{
    private readonly BookingServiceImpl _bookingService;
    private readonly Mock<IBookingRepository> _bookingRepositoryMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;

    public BookingServiceImplTests()
    {
        _eventRepositoryMock = new Mock<IEventRepository>();
        _bookingRepositoryMock = new Mock<IBookingRepository>();
        _bookingService = new BookingServiceImpl(_eventRepositoryMock.Object, _bookingRepositoryMock.Object,
            NullLogger<BookingServiceImpl>.Instance);
    }


    private Event CreateTestEvent(int id = 1, int totalSeats = 10)
    {
        var testEvent = Event.CreateInstance("Test Event", "", DateTime.Now, DateTime.Now, totalSeats);

        _eventRepositoryMock.Setup(x => x.GetEventByIdAsync(testEvent.Id, TestContext.Current.CancellationToken))
            .ReturnsAsync(testEvent);

        return testEvent;
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
    public async Task GetBookingByIdAsync_WhenBookingExists_ShouldReturnValidBooking()
    {
        var expectedBooking = Booking.CreateInstance(10);
        
        _bookingRepositoryMock.Setup(x => x.GetBookingByIdAsync(expectedBooking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBooking);

        var result = await _bookingService.GetBookingByIdAsync(expectedBooking.Id, TestContext.Current.CancellationToken);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeSameAs(expectedBooking);
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
        var testEvent = CreateTestEvent(totalSeats: expectedAvailableSeats + 1);
    
        var bookingResult = await _bookingService.CreateBookingAsync(testEvent.Id, TestContext.Current.CancellationToken);
    
        bookingResult.IsSuccess.Should().BeTrue();
        testEvent.AvailableSeats.Should().Be(expectedAvailableSeats);
    }
    
    [Fact]
    public async Task CreateBookingAsync_WhenNoSeatsAvailable_ShouldReturnConflictError()
    {
        var testEvent = CreateTestEvent(totalSeats: 1);
        var eventId = testEvent.Id;
    
        var successfulBook = await _bookingService.CreateBookingAsync(eventId, TestContext.Current.CancellationToken);
        var unsuccessfulBook = await _bookingService.CreateBookingAsync(eventId, TestContext.Current.CancellationToken);
    
        successfulBook.IsSuccess.Should().BeTrue();
        unsuccessfulBook.IsSuccess.Should().BeFalse();
        unsuccessfulBook.Error!.ErrorType.Should().Be(ErrorType.Conflict);
    }
}