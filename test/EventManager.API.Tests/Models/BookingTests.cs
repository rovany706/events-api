using EventManager.API.Models.Entities;

using FluentAssertions;

namespace EventManager.API.Tests.Models;

public class BookingTests
{
    [Fact]
    public void Confirm_Always_ShouldSetProcessedAt()
    {
        var booking = Booking.CreateInstance(1);

        booking.Confirm();

        booking.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public void Confirm_Always_ShouldSetStatus()
    {
        var booking = Booking.CreateInstance(1);
        
        booking.Confirm();

        booking.Status.Should().Be(BookingStatus.Confirmed);
    }

    [Fact]
    public void Reject_Always_ShouldSetProcessedAt()
    {
        var booking = Booking.CreateInstance(1);

        booking.Reject();

        booking.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public void Reject_Always_ShouldSetStatus()
    {
        var booking = Booking.CreateInstance(1);

        booking.Reject();

        booking.Status.Should().Be(BookingStatus.Rejected);
    }
}
