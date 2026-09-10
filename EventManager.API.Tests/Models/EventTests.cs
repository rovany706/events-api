using EventManager.API.Models.Entities;

using FluentAssertions;

namespace EventManager.API.Tests.Models;

public class EventTests
{
    private static Event CreateTestEvent(int totalSeats)
    {
        return Event.CreateInstance(1, "Test", "", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), totalSeats);
    }
    
    [Fact]
    public void AvailableSeats_Initially_ReturnsTotalSeats()
    {
        const int expected = 10;

        var eventInfo = CreateTestEvent(expected);

        eventInfo.AvailableSeats.Should().Be(expected);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void TryReserveSeats_WhenCountIsNotPositive_ThrowArgumentOutOfRangeException(int count)
    {
        var eventInfo = CreateTestEvent(10);

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
        var eventInfo = CreateTestEvent(10);
        
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
        var eventInfo = CreateTestEvent(10);
        
        int actualTrue = 0, actualFalse = 0;
        for (var i = 0; i < reserveTimes; i++)
        {
            var result = eventInfo.TryReserveSeats(reserveCount);
            actualTrue += result ? 1 : 0;
            actualFalse += result ? 0 : 1;
        }

        actualTrue.Should().Be(expectedTimesTrue);
        actualFalse.Should().Be(expectedTimesFalse);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReleaseSeats_WhenCountIsNotPositive_ThrowArgumentOutOfRangeException(int count)
    {
        var eventInfo = CreateTestEvent(10);
        
        Action act = () => eventInfo.ReleaseSeats(count);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName(nameof(count)).WithMessage("Count must be positive.*");
    }

    [Fact]
    public void ReleaseSeats_WhenCountIsGreaterThanTotalSeatCount_ThrowArgumentOutOfRangeException()
    {
        var eventInfo = CreateTestEvent(10);
        
        Action act = () => eventInfo.ReleaseSeats(eventInfo.TotalSeats + 1);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("count").WithMessage("Count must be less or equal to the total seat count.*");
    }

    [Fact]
    public void ReleaseSeats_Always_ShouldAddAvailableSeats()
    {
        const int seatCount = 5;
        const int expectedAvailableCount = 10;
        var eventInfo = CreateTestEvent(expectedAvailableCount);

        _ = eventInfo.TryReserveSeats(seatCount);
        eventInfo.ReleaseSeats(seatCount);

        eventInfo.AvailableSeats.Should().Be(expectedAvailableCount);
    }

    [Fact]
    public async Task TryReserveSeats_WhenConcurrent_ShouldReturnValidResults()
    {
        const int requestCount = 20;
        const int totalSeats = 5;
        const int expectedSuccessfulRequestCount = totalSeats;
        const int expectedUnsuccessfulRequestCount = requestCount - totalSeats;
        
        var eventToBook = CreateTestEvent(totalSeats);
        
        var tasks = new Task<bool>[requestCount];
        for (var i = 0; i < requestCount; i++)
        {
            tasks[i] = Task.Run(() => eventToBook.TryReserveSeats());
        }

        await Task.WhenAll(tasks);

        tasks.Where(x => x.Result == true).Should().HaveCount(expectedSuccessfulRequestCount);
        tasks.Where(x => x.Result == false).Should().HaveCount(expectedUnsuccessfulRequestCount);
        eventToBook.AvailableSeats.Should().Be(0);
    }
}
