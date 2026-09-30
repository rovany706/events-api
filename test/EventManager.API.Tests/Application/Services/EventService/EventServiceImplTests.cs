using EventManager.Presentation.Application.Services.EventService;
using EventManager.Presentation.Domain.Repositories.Interfaces;
using EventManager.Presentation.Models.Entities;
using EventManager.Presentation.Models.Request;
using EventManager.Presentation.Models.Results;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

namespace EventManager.API.Tests.Application.Services.EventService;

public class EventServiceImplTests
{
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly EventServiceImpl _eventService;

    public EventServiceImplTests()
    {
        _eventRepositoryMock = new Mock<IEventRepository>();
        _eventService = new EventServiceImpl(_eventRepositoryMock.Object, NullLogger<EventServiceImpl>.Instance);
    }

    private Event CreateTestEvent()
    {
        var testEvent = Event.CreateInstance(
            "Test",
            "Test",
            new DateTime(2026, 1, 1, 13, 00, 00),
            new DateTime(2026, 1, 1, 15, 00, 00),
            10);

        _eventRepositoryMock.Setup(x => x.GetEventByIdAsync(testEvent.Id, TestContext.Current.CancellationToken))
            .ReturnsAsync(testEvent);

        return testEvent;
    }
    
    [Fact]
    public async Task GetEventById_WhenEventExists_ReturnsEvent()
    {
        var expectedEventId = CreateTestEvent().Id;

        var result = await _eventService.GetEventById(expectedEventId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var actualEvent = result.Value!;
        expectedEventId.Should().Be(actualEvent.Id);
    }

    [Fact]
    public async Task GetEventById_WhenEventNotExists_ReturnsNull()
    {
        _eventRepositoryMock.Setup(x => x.GetEventByIdAsync(It.IsAny<int>(), TestContext.Current.CancellationToken))
            .ReturnsAsync((Event?)null);
        
        var result = await _eventService.GetEventById(20, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error!.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task AddEvent_ShouldAddEvent()
    {
        var createEventRequest = new CreateEventRequest
        {
            Title = "Test Event",
            Description = "Test",
            StartAt = new DateTime(2026, 1, 1, 13, 00, 00),
            EndAt = new DateTime(2026, 1, 1, 15, 00, 00),
            TotalSeats = 5
        };

        _ = await _eventService.AddEvent(createEventRequest, TestContext.Current.CancellationToken);

        _eventRepositoryMock.Verify(x => x.AddEventAsync(It.IsAny<Event>(), TestContext.Current.CancellationToken), Times.Once);
    }

    [Fact]
    public async Task TryUpdateEvent_WhenEventExists_ReturnTrueAndUpdate()
    {
        var eventToUpdateId = CreateTestEvent().Id;
        var updateRequest = new UpdateEventRequest
        {
            Title = "Updated title",
            Description = "Updated description",
            StartAt = new DateTime(2027, 1, 2, 3, 4, 5),
            EndAt = new DateTime(2027, 2, 3, 4, 5, 6)
        };

        var updateResult =
            await _eventService.TryUpdateEvent(eventToUpdateId, updateRequest, TestContext.Current.CancellationToken);
        var updatedEvent = (await _eventService.GetEventById(eventToUpdateId, TestContext.Current.CancellationToken))
            .Value!;

        _eventRepositoryMock.Verify(x => x.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
        updateResult.Should().BeTrue();
        updatedEvent.Title.Should().Be(updateRequest.Title);
        updatedEvent.Description.Should().Be(updateRequest.Description);
        updatedEvent.StartAt.Should().Be(updateRequest.StartAt);
        updatedEvent.EndAt.Should().Be(updateRequest.EndAt);
    }

    [Fact]
    public async Task TryUpdateEvent_WhenEventNotExists_ReturnFalse()
    {
        var updateRequest = new UpdateEventRequest
        {
            Title = "Updated title",
            Description = "Updated description",
            StartAt = new DateTime(2027, 1, 2, 3, 4, 5),
            EndAt = new DateTime(2027, 2, 3, 4, 5, 6)
        };

        var updateResult = await _eventService.TryUpdateEvent(10, updateRequest, TestContext.Current.CancellationToken);
        
        updateResult.Should().BeFalse();
        _eventRepositoryMock.Verify(x => x.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Never);
    }

    [Fact]
    public async Task TryRemoveEvent_WhenEventExists_ReturnTrueAndRemove()
    {
        var eventToRemove = CreateTestEvent();
        var eventToRemoveId = eventToRemove.Id;

        var removeResult = await _eventService.TryRemoveEvent(eventToRemoveId, TestContext.Current.CancellationToken);

        removeResult.Should().BeTrue();
        _eventRepositoryMock.Verify(x => x.RemoveEvent(eventToRemove), Times.Once);
        _eventRepositoryMock.Verify(x => x.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    [Fact]
    public async Task TryRemoveEvent_WhenEventNotExists_ReturnFalse()
    {
        _eventRepositoryMock.Setup(x => x.GetEventByIdAsync(It.IsAny<int>(), TestContext.Current.CancellationToken))
            .ReturnsAsync((Event?)null);

        var removeResult = await _eventService.TryRemoveEvent(10, TestContext.Current.CancellationToken);

        removeResult.Should().BeFalse();
        _eventRepositoryMock.Verify(x => x.RemoveEvent(It.IsAny<Event>()), Times.Never);
        _eventRepositoryMock.Verify(x => x.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Never);
    }
}