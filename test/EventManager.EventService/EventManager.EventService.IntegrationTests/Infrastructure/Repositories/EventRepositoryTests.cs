using EventManager.EventService.Domain.Entities;
using EventManager.EventService.Infrastructure.Persistence;
using EventManager.EventService.Infrastructure.Persistence.Repositories;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Testcontainers.PostgreSql;

namespace EventManager.EventService.IntegrationTests.Infrastructure.Repositories;

public class EventRepositoryTests : IAsyncLifetime
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

    private EventDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<EventDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        return new EventDbContext(options);
    }

    private async Task ResetDatabaseAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
    }

    [Fact]
    public async Task GetEvents_ShouldReturnEvents()
    {
        var ct = TestContext.Current.CancellationToken;
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();

        var event1 = Event.CreateInstance(
            "Event 1",
            "Description",
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 0, 1, 2), DateTimeKind.Utc),
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 1, 2, 3), DateTimeKind.Utc),
            10);

        var event2 = Event.CreateInstance(
            "Event 2",
            null,
            DateTime.SpecifyKind(new DateTime(2025, 2, 1, 0, 1, 2), DateTimeKind.Utc),
            DateTime.SpecifyKind(new DateTime(2025, 2, 2, 1, 2, 3), DateTimeKind.Utc),
            5);
        await context.Events.AddRangeAsync(event1, event2);
        await context.SaveChangesAsync(ct);

        // Act
        await using var actContext = CreateContext();
        var repository = new EventRepository(actContext);
        var events = (await repository.GetEvents(ct: ct)).Items;

        // Assert
        events.Should().BeEquivalentTo([event1, event2]);
    }

    [Fact]
    public async Task GetEventByIdAsync_WhenEventExists_ShouldReturnEvent()
    {
        var ct = TestContext.Current.CancellationToken;
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();

        var testEvent = Event.CreateInstance(
            "Event 1",
            "Description",
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 0, 1, 2), DateTimeKind.Utc),
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 1, 2, 3), DateTimeKind.Utc),
            10);
        await context.Events.AddAsync(testEvent, ct);
        await context.SaveChangesAsync(ct);

        // Act
        await using var actContext = CreateContext();
        var repository = new EventRepository(actContext);
        var actualEvent = await repository.GetEventByIdAsync(testEvent.Id, ct);

        // Assert
        actualEvent.Should().NotBeNull();
        actualEvent.Title.Should().Be(testEvent.Title);
    }

    [Fact]
    public async Task GetEventByIdAsync_WhenEventDoNotExist_ShouldReturnNull()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        await ResetDatabaseAsync();

        // Act
        await using var actContext = CreateContext();
        var repository = new EventRepository(actContext);
        var actualEvent = await repository.GetEventByIdAsync(1, ct);

        // Assert
        actualEvent.Should().BeNull();
    }

    [Fact]
    public async Task AddEventAsync_ShouldAddEventToDatabase()
    {
        var ct = TestContext.Current.CancellationToken;
        await ResetDatabaseAsync();

        // Arrange
        var testEvent = Event.CreateInstance(
            "Event 1",
            "Description",
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 0, 1, 2), DateTimeKind.Utc),
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 1, 2, 3), DateTimeKind.Utc),
            10);

        // Act
        await using var actContext = CreateContext();
        var repository = new EventRepository(actContext);
        await repository.AddEventAsync(testEvent, ct);
        await repository.SaveChangesAsync(ct);

        // Assert
        await using var verifyContext = CreateContext();
        var savedEvent = await verifyContext.Events.FirstOrDefaultAsync(e => e.Title == testEvent.Title, ct);
        savedEvent.Should().NotBeNull();
        savedEvent.Title.Should().Be(testEvent.Title);
        savedEvent.Description.Should().Be(testEvent.Description);
        savedEvent.StartAt.Should().Be(testEvent.StartAt);
        savedEvent.EndAt.Should().Be(testEvent.EndAt);
        savedEvent.TotalSeats.Should().Be(testEvent.TotalSeats);
    }

    [Fact]
    public async Task RemoveEvent_ShouldRemoveFromDatabase()
    {
        var ct = TestContext.Current.CancellationToken;
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();

        var testEvent = Event.CreateInstance(
            "Event 1",
            "Description",
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 0, 1, 2), DateTimeKind.Utc),
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 1, 2, 3), DateTimeKind.Utc),
            10);
        await context.Events.AddAsync(testEvent, ct);
        await context.SaveChangesAsync(ct);

        // Act
        await using var actContext = CreateContext();
        var repository = new EventRepository(actContext);
        var eventToRemove = await actContext.Events.FirstAsync(e => e.Title == testEvent.Title, ct);
        repository.RemoveEvent(eventToRemove);
        await repository.SaveChangesAsync(ct);

        // Assert
        await using var verifyContext = CreateContext();
        var removedEvent = await verifyContext.Events.FirstOrDefaultAsync(e => e.Title == testEvent.Title, ct);
        removedEvent.Should().BeNull();
    }
}