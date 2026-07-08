using System;
using System.Collections.Generic;
using System.Text;

using EventManager.API.Models.Entities;

using FluentAssertions;

namespace EventManager.API.Tests.Models;

public class EventTests
{
    [Fact]
    public void AvailableSeats_Initially_ReturnsTotalSeats()
    {
        const int expected = 10;

        var eventInfo = new Event
        {
            Id = 1,
            Title = "Test",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(1),
            TotalSeats = expected
        };

        eventInfo.AvailableSeats.Should().Be(expected);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void TryReserveSeats_WhenCountIsNotPositive_ThrowArgumentOutOfRangeException(int count)
    {
        var eventInfo = new Event
        {
            Id = 1,
            Title = "Test",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(1),
            TotalSeats = 10
        };

        Action act = () => eventInfo.TryReserveSeats(count);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName(nameof(count)).WithMessage("Count must be positive.*");
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(5, true)]
    [InlineData(10, true)]
    [InlineData(11, false)]
    [InlineData(20, false)]
    public void TryReserveSeats_Always_ReturnExpectedResult(int count, bool expected)
    {
        var eventInfo = new Event
        {
            Id = 1,
            Title = "Test",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(1),
            TotalSeats = 10
        };

        var actual = eventInfo.TryReserveSeats(count);

        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, 2, 2, 0)]
    [InlineData(1, 10, 10, 0)]
    [InlineData(5, 2, 2, 0)]
    [InlineData(5, 3, 2, 1)]
    [InlineData(10, 2, 1, 1)]
    [InlineData(20, 2, 0, 2)]
    public void TryReserveSeats_WhenReservedMultipleTimes_ReturnExpectedResult(int reserveCount, int reserveTimes, int expectedTimesTrue, int expectedTimesFalse)
    {
        var eventInfo = new Event
        {
            Id = 1,
            Title = "Test",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(1),
            TotalSeats = 10
        };

        int actualTrue = 0, actualFalse = 0;
        for (var i = 0; i < reserveTimes; i++)
        {
            var result = eventInfo.TryReserveSeats(reserveCount); ;
            actualTrue += Convert.ToInt32(result);
            actualFalse += Convert.ToInt32(result);
        }

        actualTrue.Should().Be(expectedTimesTrue);
        expectedTimesFalse.Should().Be(expectedTimesFalse);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReleaseSeats_WhenCountIsNotPositive_ThrowArgumentOutOfRangeException(int count)
    {
        var eventInfo = new Event
        {
            Id = 1,
            Title = "Test",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(1),
            TotalSeats = 10
        };

        Action act = () => eventInfo.ReleaseSeats(count);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName(nameof(count)).WithMessage("Count must be positive.*");
    }

    [Fact]
    public void ReleaseSeats_WhenCountIsGreaterThanTotalSeatCount_ThrowArgumentOutOfRangeException()
    {
        var eventInfo = new Event
        {
            Id = 1,
            Title = "Test",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(1),
            TotalSeats = 10
        };

        Action act = () => eventInfo.ReleaseSeats(eventInfo.TotalSeats + 1);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("count").WithMessage("Count must be less or equal to the total seat count.*");
    }

    [Fact]
    public void ReleaseSeats_Always_ShouldAddAvailableSeats()
    {
        const int seatCount = 5;
        const int expectedAvailableCount = 10;
        var eventInfo = new Event
        {
            Id = 1,
            Title = "Test",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(1),
            TotalSeats = expectedAvailableCount
        };

        _ = eventInfo.TryReserveSeats(seatCount);
        eventInfo.ReleaseSeats(seatCount);

        eventInfo.AvailableSeats.Should().Be(expectedAvailableCount);
    }
}
