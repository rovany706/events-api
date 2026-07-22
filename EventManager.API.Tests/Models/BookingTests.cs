using EventManager.API.Models.Entities;

using FluentAssertions;

namespace EventManager.API.Tests.Models;

public class BookingTests
{
    [Fact]
    public void Confirm_Always_ShouldSetProcessedAt()
    {
        var booking = new Booking
        {
            Id = 1,
            EventId = 1,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };

        booking.Confirm();

        booking.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public void Confirm_Always_ShouldSetStatus()
    {
        var booking = new Booking
        {
            Id = 1,
            EventId = 1,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };

        booking.Confirm();

        booking.Status.Should().Be(BookingStatus.Confirmed);
    }

    [Fact]
    public void Reject_Always_ShouldSetProcessedAt()
    {
        var booking = new Booking
        {
            Id = 1,
            EventId = 1,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };

        booking.Reject();

        booking.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public void Reject_Always_ShouldSetStatus()
    {
        var booking = new Booking
        {
            Id = 1,
            EventId = 1,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };

        booking.Reject();

        booking.Status.Should().Be(BookingStatus.Rejected);
    }
}
