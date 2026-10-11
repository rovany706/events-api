using EventManager.BookingService.Application.Abstractions.Persistence.Repositories;
using EventManager.BookingService.Application.Common.Results;
using EventManager.BookingService.Application.Services.BookingService;
using EventManager.BookingService.Domain.Entities;
using EventManager.BookingService.Domain.Exceptions;
using EventManager.Contracts.Jwt;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

namespace EventManager.BookingService.Application.Tests.Services.BookingService;

public class BookingServiceImplTests
{
    private readonly BookingServiceImpl _bookingService;
    private readonly Mock<IBookingRepository> _bookingRepositoryMock;

    public BookingServiceImplTests()
    {
        _bookingRepositoryMock = new Mock<IBookingRepository>();
        _bookingService =
            new BookingServiceImpl(_bookingRepositoryMock.Object, NullLogger<BookingServiceImpl>.Instance);
    }

    [Fact]
    public async Task CreateBookingAsync_WhenUserHasMaxActiveBookingCount_ShouldThrowTooManyActiveBookingsException()
    {
        const int eventId = 1;
        const int userId = 1;

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
        const int userId = 1;
        var expectedBooking = Booking.CreateInstance(10, 1);

        _bookingRepositoryMock.Setup(x => x.GetBookingByIdAsync(expectedBooking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBooking);

        var result =
            await _bookingService.GetBookingByIdAsync(expectedBooking.Id, userId, JwtRoles.UserRole,
                TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeSameAs(expectedBooking);
    }

    [Fact]
    public async Task GetBookingByIdAsync_WhenBookingDoesNotExist_ShouldReturnNotFoundError()
    {
        const int bookingId = 1;
        const int userId = 1;

        var result =
            await _bookingService.GetBookingByIdAsync(bookingId, userId, JwtRoles.UserRole,
                TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error!.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetBookingByIdAsync_WhenBookingDoesNotBelongToUser_ShouldThrowInsufficientRightsException()
    {
        const int wrongUserId = 2;
        const int userId = 1;
        var expectedBooking = Booking.CreateInstance(10, wrongUserId + 1);

        _bookingRepositoryMock.Setup(x => x.GetBookingByIdAsync(expectedBooking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBooking);

        Func<Task> act = async () =>
            await _bookingService.GetBookingByIdAsync(expectedBooking.Id, userId, JwtRoles.UserRole,
                TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InsufficientRightsException>();
    }

    [Fact]
    public async Task GetBookingByIdAsync_WhenBookingDoesNotBelongToUserAndUserIsAdmin_ShouldThrowInsufficientRightsException()
    {
        const int wrongUserId = 1;
        const int userId = 1;
        var expectedBooking = Booking.CreateInstance(10, wrongUserId + 1);

        _bookingRepositoryMock.Setup(x => x.GetBookingByIdAsync(expectedBooking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBooking);

        Func<Task> act = async () =>
            await _bookingService.GetBookingByIdAsync(expectedBooking.Id, userId, JwtRoles.AdminRole,
                TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync<InsufficientRightsException>();
    }

    [Fact]
    public async Task CancelBookingAsync_WhenBookingDoesNotExist_ShouldReturnNotFoundError()
    {
        const int bookingId = 1;
        const int userId = 1;

        var result =
            await _bookingService.CancelBookingAsync(bookingId, userId, JwtRoles.UserRole,
                TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error!.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task CancelBookingAsync_WhenBookingDoesNotBelongToUser_ShouldThrowInsufficientRightsException()
    {
        const int wrongUserId = 2;
        const int userId = 1;
        var expectedBooking = Booking.CreateInstance(10, wrongUserId + 1);

        _bookingRepositoryMock.Setup(x => x.GetBookingByIdAsync(expectedBooking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBooking);

        Func<Task> act = async () =>
            await _bookingService.CancelBookingAsync(expectedBooking.Id, userId,JwtRoles.UserRole,
                TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InsufficientRightsException>();
    }

    [Fact]
    public async Task CancelBookingAsync_WhenBookingDoesNotBelongToUserAndUserIsAdmin_ShouldThrowInsufficientRightsException()
    {
        const int wrongUserId = 1;
        const int userId = 1;
        var expectedBooking = Booking.CreateInstance(10, wrongUserId + 1);

        _bookingRepositoryMock.Setup(x => x.GetBookingByIdAsync(expectedBooking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBooking);

        Func<Task> act = async () =>
            await _bookingService.CancelBookingAsync(expectedBooking.Id, userId, JwtRoles.AdminRole,
                TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync<InsufficientRightsException>();
    }
}