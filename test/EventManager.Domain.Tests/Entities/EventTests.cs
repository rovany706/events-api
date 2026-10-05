using EventManager.Domain.Entities;
using EventManager.Domain.Exceptions;

using FluentAssertions;

namespace EventManager.Domain.Tests.Entities;

public class EventTests
{
    private static Event CreateTestEvent(int totalSeats)
    {
        return Event.CreateInstance("Test", "", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), totalSeats);
    }

    [Theory]
    [InlineData("")]
    [InlineData("    ")]
    public void CreateInstance_WhenTitleIsEmptyOrWhiteSpace_ShouldThrowEventValidationException(string title)
    {
        var startAt = new DateTime(2026, 1, 1, 1, 1, 1);

        Action act = () => Event.CreateInstance(title, "", startAt, startAt.AddDays(1), 10);

        act.Should().Throw<EventValidationException>().WithMessage("Event title is empty");
    }

    [Fact]
    public void CreateInstance_WhenStartAtAfterEndAt_ShouldThrowEventValidationException()
    {
        var startAt = new DateTime(2026, 1, 1, 1, 1, 1);

        Action act = () => Event.CreateInstance("Title", "", startAt, startAt.AddDays(-1), 10);

        act.Should().Throw<EventValidationException>().WithMessage("Event start date is greater than event end date");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void CreateInstance_WhenTotalSeatsLessThanOne_ShouldThrowEventValidationException(int totalSeats)
    {
        var startAt = new DateTime(2026, 1, 1, 1, 1, 1);

        var act = () => Event.CreateInstance("Title", "", startAt, startAt.AddDays(1), totalSeats);

        act.Should().Throw<EventValidationException>().WithMessage("Total seat count must be positive");
    }

    [Fact]
    public void CreateInstance_ShouldReturnEvent()
    {
        const string title = "Title";
        const string description = "Description";
        var startAt = new DateTime(2026, 1, 1, 1, 1, 1);
        var endAt = startAt.AddDays(1);
        const int totalSeats = 10;

        var createdEvent = Event.CreateInstance(title, description, startAt, endAt, totalSeats);

        createdEvent.Should().NotBeNull();
        createdEvent.Title.Should().Be(title);
        createdEvent.Description.Should().Be(description);
        createdEvent.StartAt.Should().Be(startAt);
        createdEvent.EndAt.Should().Be(endAt);
        createdEvent.TotalSeats.Should().Be(totalSeats);
    }

    [Fact]
    public void Update_ShouldUpdateEvent()
    {
        var startAt = new DateTime(2026, 1, 1, 1, 1, 1);
        var eventToUpdate = Event.CreateInstance("Title", "", startAt, startAt.AddDays(1), 10);

        const string newTitle = "Title";
        const string newDescription = "Description";
        DateTime newStartAt = startAt.AddDays(2);
        DateTime newEndAt = startAt.AddDays(3);

        eventToUpdate.Update(newTitle, newDescription, newStartAt, newEndAt);

        eventToUpdate.Title.Should().Be(newTitle);
        eventToUpdate.Description.Should().Be(newDescription);
        eventToUpdate.StartAt.Should().Be(newStartAt);
        eventToUpdate.EndAt.Should().Be(newEndAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("    ")]
    public void Update_WhenTitleIsEmptyOrWhiteSpace_ShouldThrowEventValidationException(string title)
    {
        var startAt = new DateTime(2026, 1, 1, 1, 1, 1);
        var eventToUpdate = Event.CreateInstance("Title", "", startAt, startAt.AddDays(1), 10);

        Action act = () => eventToUpdate.Update(title, "", startAt, startAt.AddDays(1));

        act.Should().Throw<EventValidationException>().WithMessage("Event title is empty");
    }

    [Fact]
    public void Update_WhenStartAtAfterEndAt_ShouldThrowEventValidationException()
    {
        var startAt = new DateTime(2026, 1, 1, 1, 1, 1);
        var eventToUpdate = Event.CreateInstance("Title", "", startAt, startAt.AddDays(1), 10);

        Action act = () => eventToUpdate.Update("Title", "", startAt, startAt.AddDays(-1));

        act.Should().Throw<EventValidationException>().WithMessage("Event start date is greater than event end date");
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

        var results = await Task.WhenAll(tasks);

        results.Where(x => x).Should().HaveCount(expectedSuccessfulRequestCount);
        results.Where(x => !x).Should().HaveCount(expectedUnsuccessfulRequestCount);
        eventToBook.AvailableSeats.Should().Be(0);
    }
}