using EventManager.Application.Abstractions.Services.Dto;
using EventManager.Domain.Common.Pagination;
using EventManager.Infrastructure.Persistence;
using EventManager.Infrastructure.Persistence.Repositories;
using EventManager.Infrastructure.Services.EventService;
using EventManager.IntegrationTests.Application.Services.EventService.TestHelpers;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

using Testcontainers.PostgreSql;

namespace EventManager.IntegrationTests.Application.Services.EventService;

public class EventServiceImplTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("test_db")
        .Build();

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        return new AppDbContext(options);
    }

    private async Task ResetDatabaseAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
    }

    [Fact]
    public async Task AddEvent_ShouldAddEventAndReturnNewId()
    {
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();
        var repository = new EventRepository(context);
        var eventService = new EventServiceImpl(repository, NullLogger<EventServiceImpl>.Instance);

        // Act
        var createEventRequest = new CreateEventRequest
        {
            Title = "Test Event",
            Description = "Test",
            StartAt = DateTime.SpecifyKind(new DateTime(2026, 1, 1, 13, 00, 00), DateTimeKind.Utc),
            EndAt = DateTime.SpecifyKind(new DateTime(2026, 1, 1, 15, 00, 00), DateTimeKind.Utc),
            TotalSeats = 5
        };

        await eventService.AddEvent(createEventRequest, TestContext.Current.CancellationToken);

        // Assert
        await using var verifyContext = CreateContext();
        var addedEvent = await verifyContext.Events.FirstOrDefaultAsync(e => e.Title == createEventRequest.Title,
            TestContext.Current.CancellationToken);

        addedEvent.Should().NotBeNull();
        addedEvent.Id.Should().Be(1);
        addedEvent.Title.Should().Be(createEventRequest.Title);
        addedEvent.Description.Should().Be(createEventRequest.Description);
        addedEvent.StartAt.Should().Be(createEventRequest.StartAt);
        addedEvent.EndAt.Should().Be(createEventRequest.EndAt);
        addedEvent.TotalSeats.Should().Be(createEventRequest.TotalSeats);
    }

    [Fact]
    [Trait("Category", "Filters")]
    public async Task GetEvents_WhenFiltersAreEmpty_ReturnsAllEvents()
    {
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();
        context.Events.AddRange(EventTestDataGenerator.GetTestEvents());
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        // Act
        var repository = new EventRepository(context);
        var eventService = new EventServiceImpl(repository, NullLogger<EventServiceImpl>.Instance);
        var events = await eventService.GetEvents(new EventFilterDto(), new PaginationParams { PageSize = 100 },
            TestContext.Current.CancellationToken);

        // Assert
        events.Should().NotBeNull();
        events.Items.Should().HaveCount(15);
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
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();
        context.Events.AddRange(EventTestDataGenerator.GetTestEvents());
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        // Act
        var repository = new EventRepository(context);
        var eventService = new EventServiceImpl(repository, NullLogger<EventServiceImpl>.Instance);
        
        var events = await eventService.GetEvents(new EventFilterDto { Title = titleFilter },
            new PaginationParams { PageSize = 100 }, TestContext.Current.CancellationToken);

        // Assert
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
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();
        context.Events.AddRange(EventTestDataGenerator.GetTestEvents());
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        // Act
        var repository = new EventRepository(context);
        var eventService = new EventServiceImpl(repository, NullLogger<EventServiceImpl>.Instance);
        var events = await eventService.GetEvents(
            new EventFilterDto { From = DateTime.SpecifyKind(new DateTime(year, month, day, hour, 0, 0), DateTimeKind.Utc) },
            new PaginationParams { PageSize = 100 }, TestContext.Current.CancellationToken);

        // Assert
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
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();
        context.Events.AddRange(EventTestDataGenerator.GetTestEvents());
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        // Act
        var repository = new EventRepository(context);
        var eventService = new EventServiceImpl(repository, NullLogger<EventServiceImpl>.Instance);
        var events = await eventService.GetEvents(
            new EventFilterDto { To = DateTime.SpecifyKind(new DateTime(year, month, day, hour, 0, 0), DateTimeKind.Utc) },
            new PaginationParams { PageSize = 100 }, TestContext.Current.CancellationToken);

        // Assert
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
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();
        context.Events.AddRange(EventTestDataGenerator.GetTestEvents().Take(initialCount));
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        // Act
        var repository = new EventRepository(context);
        var eventService = new EventServiceImpl(repository, NullLogger<EventServiceImpl>.Instance);
        var pagedEvents = await eventService.GetEvents(
            new EventFilterDto(),
            new PaginationParams { Page = page, PageSize = pageSize }, TestContext.Current.CancellationToken);

        // Assert
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
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();
        context.Events.AddRange(EventTestDataGenerator.GetTestEvents());
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        // Act
        var repository = new EventRepository(context);
        var eventService = new EventServiceImpl(repository, NullLogger<EventServiceImpl>.Instance);
        var filter = new EventFilterDto { Title = title, From = from, To = to };

        var filteredEvents = await eventService.GetEvents(filter, new PaginationParams { PageSize = 100 },
            TestContext.Current.CancellationToken);

        // Assert
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