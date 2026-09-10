using EventManager.API.Application.Services.EventService;
using EventManager.API.Application.Services.EventService.Models;
using EventManager.API.Domain.DataAccess;
using EventManager.API.Models.Entities;
using EventManager.API.Models.Request;
using EventManager.API.Models.Results;
using EventManager.API.Tests.Models;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EventManager.API.Tests.Application.Services.EventService;

public class EventServiceImplTests : IDisposable
{
    private readonly IEventService _eventService;
    private readonly IServiceScope _scope;
    private readonly ServiceProvider _serviceProvider;
    private readonly IEnumerable<Event> _mockEvents = EventTestDataGenerator.GetTestEvents();

    public EventServiceImplTests()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));
        services.AddScoped<IEventService, EventServiceImpl>();
        services.AddLogging(l => l.AddProvider(NullLoggerProvider.Instance));

        _serviceProvider = services.BuildServiceProvider();
        _scope = _serviceProvider.CreateScope();
        _eventService = _scope.ServiceProvider.GetRequiredService<IEventService>();
    }

    public void Dispose()
    {
        _scope.Dispose();
        _serviceProvider.Dispose();
    }

    private async Task<int> CreateTestEvent()
    {
        var id = await _eventService.AddEvent(
            new CreateEventRequest
            {
                Title = "Test",
                Description = "Test",
                StartAt = new DateTime(2026, 1, 1, 13, 00, 00),
                EndAt = new DateTime(2026, 1, 1, 15, 00, 00),
                TotalSeats = 10
            }, TestContext.Current.CancellationToken);

        return id;
    }

    private async Task FillTestDatabase(int count = 15)
    {
        foreach (var mockEvent in _mockEvents.Take(count))
        {
            _ = await _eventService.AddEvent(
                new CreateEventRequest
                {
                    Title = mockEvent.Title,
                    Description = mockEvent.Description,
                    StartAt = mockEvent.StartAt,
                    EndAt = mockEvent.EndAt,
                    TotalSeats = mockEvent.TotalSeats
                }, TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    [Trait("Category", "Filters")]
    public async Task GetEvents_WhenFiltersAreEmpty_ReturnsAllEvents()
    {
        await FillTestDatabase();

        var events = await _eventService.GetEvents(new EventFilterDto(), new PaginationParams { PageSize = 100 },
            TestContext.Current.CancellationToken);

        events.Should().NotBeNull();
        events.Items.Should().NotBeEmpty().And.BeEquivalentTo(_mockEvents);
    }

    [Fact]
    public async Task GetEventById_WhenEventExists_ReturnsEvent()
    {
        var expectedEventId = await CreateTestEvent();

        var result = await _eventService.GetEventById(expectedEventId, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var actualEvent = result.Value!;
        expectedEventId.Should().Be(actualEvent.Id);
    }

    [Fact]
    public async Task GetEventById_WhenEventNotExists_ReturnsNull()
    {
        var result = await _eventService.GetEventById(20, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error!.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task AddEvent_ShouldAddEventAndReturnNewId()
    {
        const int expectedEventId = 1;
        var createEventRequest = new CreateEventRequest
        {
            Title = "Test Event",
            Description = "Test",
            StartAt = new DateTime(2026, 1, 1, 13, 00, 00),
            EndAt = new DateTime(2026, 1, 1, 15, 00, 00),
            TotalSeats = 5
        };

        var actualId = await _eventService.AddEvent(createEventRequest, TestContext.Current.CancellationToken);
        var eventResult = await _eventService.GetEventById(expectedEventId, TestContext.Current.CancellationToken);

        eventResult.IsSuccess.Should().BeTrue();
        var addedEvent = eventResult.Value!;
        actualId.Should().Be(expectedEventId);
        addedEvent.Id.Should().Be(expectedEventId);
        addedEvent.Title.Should().Be(createEventRequest.Title);
        addedEvent.Description.Should().Be(createEventRequest.Description);
        addedEvent.StartAt.Should().Be(createEventRequest.StartAt);
        addedEvent.EndAt.Should().Be(createEventRequest.EndAt);
        addedEvent.TotalSeats.Should().Be(createEventRequest.TotalSeats);
    }

    [Fact]
    public async Task TryUpdateEvent_WhenEventExists_ReturnTrueAndUpdate()
    {
        var eventToUpdateId = await CreateTestEvent();
        var updateRequest = new UpdateEventRequest
        {
            Title = "Updated title",
            Description = "Updated description",
            StartAt = new DateTime(2027, 1, 2, 3, 4, 5),
            EndAt = new DateTime(2027, 2, 3, 4, 5, 6)
        };

        var updateResult =
            await _eventService.TryUpdateEvent(eventToUpdateId, updateRequest, TestContext.Current.CancellationToken);
        var updatedEvent = (await _eventService.GetEventById(eventToUpdateId, TestContext.Current.CancellationToken)).Value!;
        
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
    }

    [Fact]
    public async Task TryRemoveEvent_WhenEventExists_ReturnTrueAndRemove()
    {
        var eventToRemoveId = await CreateTestEvent();

        var removeResult = await _eventService.TryRemoveEvent(eventToRemoveId, TestContext.Current.CancellationToken);
        var eventResult = await _eventService.GetEventById(eventToRemoveId, TestContext.Current.CancellationToken);
        
        removeResult.Should().BeTrue();
        eventResult.IsSuccess.Should().BeFalse();
        eventResult.Error!.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task TryRemoveEvent_WhenEventNotExists_ReturnFalse()
    {
        var removeResult = await _eventService.TryRemoveEvent(10, TestContext.Current.CancellationToken);

        removeResult.Should().BeFalse();
    }

    [Theory]
    [Trait("Category", "Filters")]
    [InlineData("Conference", 2)]
    [InlineData("TECH", 1)]
    [InlineData("2026", 2)]
    [InlineData("2025", 0)]
    [InlineData("", 15)]
    [InlineData("   ", 15)]
    public async Task GetEvents_WhenFilteredByTitle_ReturnExpectedResults(string titleFilter, int expectedCount)
    {
        await FillTestDatabase();

        var events = await _eventService.GetEvents(new EventFilterDto { Title = titleFilter },
            new PaginationParams { PageSize = 100 }, TestContext.Current.CancellationToken);

        events.ItemCount.Should().Be(expectedCount);
    }

    [Theory]
    [Trait("Category", "Filters")]
    [InlineData(2026, 6, 7, 12, 7)]
    [InlineData(2026, 6, 6, 12, 11)] // -1 day
    [InlineData(2026, 6, 8, 12, 5)] // +1 day
    public async Task GetEvents_WhenFilteredByFrom_ReturnExpectedResults(int year, int month, int day, int hour,
        int expectedCount)
    {
        await FillTestDatabase();

        var events = await _eventService.GetEvents(
            new EventFilterDto { From = new DateTime(year, month, day, hour, 0, 0) },
            new PaginationParams { PageSize = 100 }, TestContext.Current.CancellationToken);

        events.ItemCount.Should().Be(expectedCount);
    }

    [Theory]
    [Trait("Category", "Filters")]
    [InlineData(2026, 6, 7, 12, 5)]
    [InlineData(2026, 6, 6, 12, 3)] // -1 day
    [InlineData(2026, 6, 8, 12, 9)] // +1 day
    public async Task GetEvents_WhenFilteredByTo_ReturnExpectedResults(int year, int month, int day, int hour,
        int expectedCount)
    {
        await FillTestDatabase();

        var events = await _eventService.GetEvents(
            new EventFilterDto { To = new DateTime(year, month, day, hour, 0, 0) },
            new PaginationParams { PageSize = 100 }, TestContext.Current.CancellationToken);

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
    public async Task GetEvents_WhenPaginated_ReturnExpectedResults(int page, int pageSize, int initialCount,
        int expectedItemCount, int expectedPage, int expectedTotalPages, int expectedTotalItems)
    {
        await FillTestDatabase(initialCount);

        var pagedEvents = await _eventService.GetEvents(
            new EventFilterDto(),
            new PaginationParams { Page = page, PageSize = pageSize }, TestContext.Current.CancellationToken);

        pagedEvents.ItemCount.Should().Be(expectedItemCount);
        pagedEvents.CurrentPage.Should().Be(expectedPage);
        pagedEvents.TotalPages.Should().Be(expectedTotalPages);
        pagedEvents.TotalItems.Should().Be(expectedTotalItems);
    }

    [Theory]
    [Trait("Category", "Filters")]
    [MemberData(nameof(TestEventFilters))]
    public async Task GetEvents_WhenFiltered_ReturnExpectedResults(string title, DateTime from, DateTime to,
        int expectedCount)
    {
        await FillTestDatabase();

        var filter = new EventFilterDto { Title = title, From = from, To = to };

        var filteredEvents = await _eventService.GetEvents(filter, new PaginationParams { PageSize = 100 },
            TestContext.Current.CancellationToken);

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