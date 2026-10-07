using EventManager.Domain.Entities.Bookings;
using EventManager.Domain.Exceptions;

using FluentAssertions;

namespace EventManager.Domain.Tests.Entities.Bookings;

public class BookingTests
{
    [Fact]
    public void Confirm_Always_ShouldSetProcessedAt()
    {
        var booking = Booking.CreateInstance(1, 1);

        booking.Confirm();

        booking.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public void Confirm_Always_ShouldSetStatus()
    {
        var booking = Booking.CreateInstance(1, 1);
        
        booking.Confirm();

        booking.Status.Should().Be(BookingStatus.Confirmed);
    }

    [Fact]
    public void Reject_Always_ShouldSetProcessedAt()
    {
        var booking = Booking.CreateInstance(1, 1);

        booking.Reject();

        booking.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public void Reject_Always_ShouldSetStatus()
    {
        var booking = Booking.CreateInstance(1, 1);

        booking.Reject();

        booking.Status.Should().Be(BookingStatus.Rejected);
    }

    [Fact]
    public void Cancel_Always_ShouldSetStatus()
    {
        var booking = Booking.CreateInstance(1, 1);

        booking.Cancel();

        booking.Status.Should().Be(BookingStatus.Cancelled);
    }
    
    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldThrowBookingAlreadyCancelledException()
    {
        var booking = Booking.CreateInstance(1, 1);

        booking.Cancel();
        Action act = booking.Cancel;

        act.Should().Throw<BookingAlreadyCancelledException>();
    }
}
