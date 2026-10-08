using EventManager.Application.Abstractions.Persistence.Repositories;
using EventManager.Application.Common.Results;
using EventManager.Application.Services.BookingService;
using EventManager.Domain.Entities;
using EventManager.Domain.Entities.Bookings;
using EventManager.Domain.Entities.Users;
using EventManager.Domain.Exceptions;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

namespace EventManager.Application.Tests.Services.BookingService;

public class BookingServiceImplTests
{
    private readonly BookingServiceImpl _bookingService;
    private readonly Mock<IBookingRepository> _bookingRepositoryMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;

    public BookingServiceImplTests()
    {
        _eventRepositoryMock = new Mock<IEventRepository>();
        _bookingRepositoryMock = new Mock<IBookingRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _bookingService = new BookingServiceImpl(_eventRepositoryMock.Object, _bookingRepositoryMock.Object,
            _userRepositoryMock.Object, NullLogger<BookingServiceImpl>.Instance);
    }


    private Event CreateTestEvent(int totalSeats = 10)
    {
        var testEvent = Event.CreateInstance("Test Event", "", DateTime.Now, DateTime.Now, totalSeats);

        _eventRepositoryMock.Setup(x => x.GetEventByIdAsync(testEvent.Id, TestContext.Current.CancellationToken))
            .ReturnsAsync(testEvent);

        return testEvent;
    }

    private User CreateTestUser(int id = 1, UserRole role = UserRole.User)
    {
        var testUser = User.CreateInstance(id, "user", "123", role);

        _userRepositoryMock.Setup(x => x.GetUserByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(testUser);

        return testUser;
    }

    [Fact]
    public async Task CreateBookingAsync_WhenEventDoesNotExist_ShouldReturnNotFoundError()
    {
        const int eventId = 1;
        const int userId = 1;
        _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId, TestContext.Current.CancellationToken))
            .ReturnsAsync(CreateTestUser());

        var bookingResult =
            await _bookingService.CreateBookingAsync(eventId, userId, TestContext.Current.CancellationToken);

        bookingResult.IsSuccess.Should().BeFalse();
        bookingResult.Error!.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task CreateBookingAsync_WhenUserDoesNotExist_ShouldReturnNotFoundError()
    {
        const int eventId = 1;

        var bookingResult = await _bookingService.CreateBookingAsync(eventId, 1, TestContext.Current.CancellationToken);

        bookingResult.IsSuccess.Should().BeFalse();
        bookingResult.Error!.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task CreateBookingAsync_WhenUserHasMaxActiveBookingCount_ShouldThrowTooManyActiveBookingsException()
    {
        const int eventId = 1;
        var userId = CreateTestUser().Id;

        _bookingRepositoryMock
            .Setup(x => x.GetActiveBookingCountForUserAsync(userId, TestContext.Current.CancellationToken))
            .ReturnsAsync(BookingConstants.MaxActiveBookingCountPerUser);

        Func<Task> act = () =>
            _bookingService.CreateBookingAsync(eventId, userId, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<TooManyActiveBookingsException>();
        _bookingRepositoryMock.Verify(
            x => x.GetActiveBookingCountForUserAsync(userId, TestContext.Current.CancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetBookingByIdAsync_WhenBookingAndUserExists_ShouldReturnValidBooking()
    {
        var userId = CreateTestUser().Id;
        var expectedBooking = Booking.CreateInstance(10, 1);

        _bookingRepositoryMock.Setup(x => x.GetBookingByIdAsync(expectedBooking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBooking);

        var result =
            await _bookingService.GetBookingByIdAsync(expectedBooking.Id, userId,
                TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeSameAs(expectedBooking);
    }

    [Fact]
    public async Task GetBookingByIdAsync_WhenBookingDoesNotExist_ShouldReturnNotFoundError()
    {
        const int bookingId = 1;
        var userId = CreateTestUser().Id;

        var result =
            await _bookingService.GetBookingByIdAsync(bookingId, userId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error!.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetBookingByIdAsync_WhenUserDoesNotExist_ShouldReturnNotFoundError()
    {
        const int userId = 1;
        const int bookingId = 1;
        _bookingRepositoryMock.Setup(x => x.GetBookingByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Booking.CreateInstance(1, userId));

        var result =
            await _bookingService.GetBookingByIdAsync(bookingId, userId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error!.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task CreateBookingAsync_Always_ShouldDecreaseEventAvailableSeats()
    {
        const int expectedAvailableSeats = 2;
        var userId = CreateTestUser().Id;
        var testEvent = CreateTestEvent(totalSeats: expectedAvailableSeats + 1);

        var bookingResult =
            await _bookingService.CreateBookingAsync(testEvent.Id, userId, TestContext.Current.CancellationToken);

        bookingResult.IsSuccess.Should().BeTrue();
        testEvent.AvailableSeats.Should().Be(expectedAvailableSeats);
    }

    [Fact]
    public async Task CreateBookingAsync_WhenNoSeatsAvailable_ShouldReturnConflictError()
    {
        var userId = CreateTestUser().Id;
        var eventId = CreateTestEvent(totalSeats: 1).Id;

        var successfulBook =
            await _bookingService.CreateBookingAsync(eventId, userId, TestContext.Current.CancellationToken);
        var unsuccessfulBook =
            await _bookingService.CreateBookingAsync(eventId, userId, TestContext.Current.CancellationToken);

        successfulBook.IsSuccess.Should().BeTrue();
        unsuccessfulBook.IsSuccess.Should().BeFalse();
        unsuccessfulBook.Error!.ErrorType.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task GetBookingByIdAsync_WhenBookingDoesNotBelongToUser_ShouldThrowInsufficientRightsException()
    {
        const int wrongUserId = 2;
        var userId = CreateTestUser(wrongUserId).Id;
        var expectedBooking = Booking.CreateInstance(10, wrongUserId + 1);

        _bookingRepositoryMock.Setup(x => x.GetBookingByIdAsync(expectedBooking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBooking);

        Func<Task> act = async () =>
            await _bookingService.GetBookingByIdAsync(expectedBooking.Id, userId,
                TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InsufficientRightsException>();
    }

    [Fact]
    public async Task GetBookingByIdAsync_WhenBookingDoesNotBelongToUserAndUserIsAdmin_ShouldThrowInsufficientRightsException()
    {
        const int wrongUserId = 1;
        var userId = CreateTestUser(wrongUserId, UserRole.Admin).Id;
        var expectedBooking = Booking.CreateInstance(10, wrongUserId + 1);

        _bookingRepositoryMock.Setup(x => x.GetBookingByIdAsync(expectedBooking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBooking);

        Func<Task> act = async () =>
            await _bookingService.GetBookingByIdAsync(expectedBooking.Id, userId,
                TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync<InsufficientRightsException>();
    }

    [Fact]
    public async Task CancelBookingAsync_WhenBookingDoesNotExist_ShouldReturnNotFoundError()
    {
        const int bookingId = 1;
        var userId = CreateTestUser().Id;

        var result =
            await _bookingService.CancelBookingAsync(bookingId, userId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error!.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task CancelBookingAsync_WhenUserDoesNotExist_ShouldReturnNotFoundError()
    {
        const int userId = 1;
        const int bookingId = 1;
        _bookingRepositoryMock.Setup(x => x.GetBookingByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Booking.CreateInstance(1, userId));

        var result =
            await _bookingService.CancelBookingAsync(bookingId, userId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error!.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task CancelBookingAsync_WhenBookingDoesNotBelongToUser_ShouldThrowInsufficientRightsException()
    {
        const int wrongUserId = 2;
        var userId = CreateTestUser(wrongUserId).Id;
        var expectedBooking = Booking.CreateInstance(10, wrongUserId + 1);

        _bookingRepositoryMock.Setup(x => x.GetBookingByIdAsync(expectedBooking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBooking);

        Func<Task> act = async () =>
            await _bookingService.CancelBookingAsync(expectedBooking.Id, userId,
                TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InsufficientRightsException>();
    }

    [Fact]
    public async Task CancelBookingAsync_WhenBookingDoesNotBelongToUserAndUserIsAdmin_ShouldThrowInsufficientRightsException()
    {
        const int wrongUserId = 1;
        var userId = CreateTestUser(wrongUserId, UserRole.Admin).Id;
        var expectedBooking = Booking.CreateInstance(10, wrongUserId + 1);

        _bookingRepositoryMock.Setup(x => x.GetBookingByIdAsync(expectedBooking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBooking);

        Func<Task> act = async () =>
            await _bookingService.CancelBookingAsync(expectedBooking.Id, userId,
                TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync<InsufficientRightsException>();
    }
}