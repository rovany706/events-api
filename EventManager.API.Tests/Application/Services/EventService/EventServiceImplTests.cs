using EventManager.API.Application.Services.EventService;
using EventManager.API.Application.Services.EventService.Models;
using EventManager.API.Domain.Interfaces;
using EventManager.API.Models.Entities;
using EventManager.API.Models.Request;
using EventManager.API.Models.Results;
using EventManager.API.Tests.Models;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

namespace EventManager.API.Tests.Application.Services.EventService;

public class EventServiceImplTests
{
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly EventServiceImpl _eventService;
    private readonly IEnumerable<Event> _mockEvents = EventTestDataGenerator.GetTestEvents();

    public EventServiceImplTests()
    {
        _eventRepositoryMock = new Mock<IEventRepository>();
        _eventService = new EventServiceImpl(_eventRepositoryMock.Object, NullLogger<EventServiceImpl>.Instance);
    }

    private Event GetTestEvent()
    {
        return new Event
        {
            Id = 20,
            Title = "Test",
            StartAt = new DateTime(2026, 1, 1, 13, 00, 00),
            EndAt = new DateTime(2026, 1, 1, 15, 00, 00),
            Description = "Test",
            TotalSeats = 10
        };
    }

    [Fact]
    [Trait("Category", "Filters")]
    public void GetEvents_WhenFiltersAreEmpty_ReturnsAllEvents()
    {
        _eventRepositoryMock.Setup(x => x.GetEvents()).Returns(_mockEvents);

        var events = _eventService.GetEvents(new EventFilterDto(), new PaginationParams { PageSize = 100 });

        events.Should().NotBeNull();
        events.Items.Should().NotBeEmpty().And.BeEqualTo(_mockEvents);
    }

    [Fact]
    public void GetEventById_WhenEventExists_ReturnsEvent()
    {
        var expectedEvent = GetTestEvent();

        _eventRepositoryMock.Setup(x => x.GetEventById(expectedEvent.Id)).Returns(expectedEvent);

        var result = _eventService.GetEventById(expectedEvent.Id);
        
        result.IsSuccess.Should().BeTrue();
        var actualEvent = result.Value;
        expectedEvent.Should().Be(actualEvent);
    }

    [Fact]
    public void GetEventById_WhenEventNotExists_ReturnsNull()
    {
        _eventRepositoryMock.Setup(x => x.GetEventById(It.IsAny<int>())).Returns((Event?)null);

        var result = _eventService.GetEventById(20);

        result.IsSuccess.Should().BeFalse();
        result.Error!.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void AddEvent_ShouldAddEventAndReturnNewId()
    {
        const int expectedId = 20;
        var newEvent = GetTestEvent();
        _eventRepositoryMock.Setup(x => x.AddEvent(newEvent)).Returns(expectedId);
        _eventRepositoryMock.Setup(x => x.GetEventById(expectedId)).Returns(newEvent with { Id = expectedId });

        var actualId = _eventService.AddEvent(newEvent);
        var eventResult = _eventService.GetEventById(expectedId);
        
        eventResult.IsSuccess.Should().BeTrue();
        var addedEvent = eventResult.Value;
        actualId.Should().Be(expectedId);
        addedEvent.Should().Be(newEvent with { Id = expectedId });
    }

    [Fact]
    public void TryUpdateEvent_WhenEventExists_ReturnTrueAndUpdate()
    {
        var updatedEvent = GetTestEvent();
        _eventRepositoryMock.Setup(x => x.GetEventById(updatedEvent.Id)).Returns(updatedEvent);

        var updateResult = _eventService.TryUpdateEvent(updatedEvent);

        updateResult.Should().BeTrue();
        _eventRepositoryMock.Verify(x => x.UpdateEvent(updatedEvent), Times.Once);
    }

    [Fact]
    public void TryUpdateEvent_WhenEventNotExists_ReturnFalse()
    {
        var updatedEvent = GetTestEvent();
        _eventRepositoryMock.Setup(x => x.GetEventById(It.IsAny<int>())).Returns((Event?)null);

        var updateResult = _eventService.TryUpdateEvent(updatedEvent);

        updateResult.Should().BeFalse();
        _eventRepositoryMock.Verify(x => x.UpdateEvent(It.IsAny<Event>()), Times.Never);
    }

    [Fact]
    public void TryRemoveEvent_WhenEventExists_ReturnTrueAndRemove()
    {
        var eventToRemove = GetTestEvent();
        _eventRepositoryMock.Setup(x => x.GetEventById(eventToRemove.Id)).Returns(eventToRemove);
        _eventRepositoryMock.Setup(x => x.RemoveEvent(eventToRemove)).Returns(true);

        var removeResult = _eventService.TryRemoveEvent(eventToRemove.Id);

        removeResult.Should().BeTrue();
        _eventRepositoryMock.Verify(x => x.RemoveEvent(eventToRemove), Times.Once);
    }

    [Fact]
    public void TryRemoveEvent_WhenEventNotExists_ReturnFalse()
    {
        var idToRemove = 20;
        _eventRepositoryMock.Setup(x => x.GetEventById(idToRemove)).Returns((Event?)null);

        var removeResult = _eventService.TryRemoveEvent(idToRemove);

        removeResult.Should().BeFalse();
        _eventRepositoryMock.Verify(x => x.RemoveEvent(It.IsAny<Event>()), Times.Never);
    }

    [Fact]
    public void TryRemoveEvent_WhenEventExistsAndRemoveFailed_ReturnFalse()
    {
        var eventToRemove = GetTestEvent();
        _eventRepositoryMock.Setup(x => x.GetEventById(eventToRemove.Id)).Returns(eventToRemove);
        _eventRepositoryMock.Setup(x => x.RemoveEvent(eventToRemove)).Returns(false);

        var removeResult = _eventService.TryRemoveEvent(eventToRemove.Id);

        removeResult.Should().BeFalse();
        _eventRepositoryMock.Verify(x => x.RemoveEvent(eventToRemove), Times.Once);
    }

    [Theory]
    [Trait("Category", "Filters")]
    [InlineData("Conference", 2)]
    [InlineData("TECH", 1)]
    [InlineData("2026", 2)]
    [InlineData("2025", 0)]
    [InlineData("", 15)]
    [InlineData("   ", 15)]
    public void GetEvents_WhenFilteredByTitle_ReturnExpectedResults(string titleFilter, int expectedCount)
    {
        _eventRepositoryMock.Setup(x => x.GetEvents()).Returns(_mockEvents);

        var events = _eventService.GetEvents(new EventFilterDto { Title = titleFilter },
            new PaginationParams { PageSize = 100 });

        events.ItemCount.Should().Be(expectedCount);
    }

    [Theory]
    [Trait("Category", "Filters")]
    [InlineData(2026, 6, 7, 12, 7)]
    [InlineData(2026, 6, 6, 12, 11)] // -1 day
    [InlineData(2026, 6, 8, 12, 5)] // +1 day
    public void GetEvents_WhenFilteredByFrom_ReturnExpectedResults(int year, int month, int day, int hour,
        int expectedCount)
    {
        _eventRepositoryMock.Setup(x => x.GetEvents()).Returns(_mockEvents);

        var events = _eventService.GetEvents(
            new EventFilterDto { From = new DateTime(year, month, day, hour, 0, 0) },
            new PaginationParams { PageSize = 100 }
        );

        events.ItemCount.Should().Be(expectedCount);
    }

    [Theory]
    [Trait("Category", "Filters")]
    [InlineData(2026, 6, 7, 12, 5)]
    [InlineData(2026, 6, 6, 12, 3)] // -1 day
    [InlineData(2026, 6, 8, 12, 9)] // +1 day
    public void GetEvents_WhenFilteredByTo_ReturnExpectedResults(int year, int month, int day, int hour,
        int expectedCount)
    {
        _eventRepositoryMock.Setup(x => x.GetEvents()).Returns(_mockEvents);

        var events = _eventService.GetEvents(
            new EventFilterDto { To = new DateTime(year, month, day, hour, 0, 0) },
            new PaginationParams { PageSize = 100 }
        );

        events.ItemCount.Should().Be(expectedCount);
    }

    [Theory]
    [Trait("Category", "Pagination")]
    [InlineData(1, 10, 15, 10, 1, 2, 15)]
    [InlineData(2, 10, 15, 5, 2, 2, 15)]
    [InlineData(3, 10, 15, 0, 3, 2, 15)]
    [InlineData(1, 20, 15, 15, 1, 1, 15)]
    [InlineData(1, 10, 10, 10, 1, 1, 10)]
    [InlineData(1, 10, 0, 0, 1, 0, 0)]
    public void GetEvents_WhenPaginated_ReturnExpectedResults(int page, int pageSize, int initialCount,
        int expectedItemCount, int expectedPage, int expectedTotalPages, int expectedTotalItems)
    {
        var events = _mockEvents.Take(initialCount);
        _eventRepositoryMock.Setup(x => x.GetEvents()).Returns(events);

        var pagedEvents = _eventService.GetEvents(
            new EventFilterDto(),
            new PaginationParams { Page = page, PageSize = pageSize }
        );

        pagedEvents.ItemCount.Should().Be(expectedItemCount);
        pagedEvents.CurrentPage.Should().Be(expectedPage);
        pagedEvents.TotalPages.Should().Be(expectedTotalPages);
        pagedEvents.TotalItems.Should().Be(expectedTotalItems);
    }

    [Theory]
    [Trait("Category", "Filters")]
    [MemberData(nameof(TestEventFilters))]
    public void GetEvents_WhenFiltered_ReturnExpectedResults(string title, DateTime from, DateTime to,
        int expectedCount)
    {
        var filter = new EventFilterDto { Title = title, From = from, To = to };
        _eventRepositoryMock.Setup(x => x.GetEvents()).Returns(_mockEvents);

        var filteredEvents = _eventService.GetEvents(filter, new PaginationParams { PageSize = 100 });

        filteredEvents.ItemCount.Should().Be(expectedCount);
    }

    public static IEnumerable<TheoryDataRow<string, DateTime, DateTime, int>> TestEventFilters()
    {
        var now = EventTestDataGenerator.Now;

        return
        [
            new TheoryDataRow<string, DateTime, DateTime, int>(
                "",
                now.AddDays(-10),
                now,
                5 // Ids: 1,2,3,13,15
            ),
            new TheoryDataRow<string, DateTime, DateTime, int>(
                "Conference",
                now.AddDays(-10),
                now,
                2 // Ids: 1,2
            ),
            new TheoryDataRow<string, DateTime, DateTime, int>(
                "now",
                now,
                now.AddHours(4),
                1 // Ids: 7
            ),
            // Only today
            new TheoryDataRow<string, DateTime, DateTime, int>(
                "",
                now.AddHours(-12),
                now.AddHours(12),
                3 // Ids: 7, 13, 14
            ),
            // All future events
            new TheoryDataRow<string, DateTime, DateTime, int>(
                "",
                now,
                now.AddYears(1),
                7 // Ids: 7,8,9,10,11,12,14
            ),
            // From > To
            new TheoryDataRow<string, DateTime, DateTime, int>(
                "",
                now.AddDays(1),
                now.AddDays(-1),
                0
            )
        ];
    }
}