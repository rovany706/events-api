using EventManager.API.Application.Services.EventService;
using EventManager.API.Domain.DataAccess;
using EventManager.API.Domain.Repositories;
using EventManager.API.Models.Request;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

using Npgsql;

using Testcontainers.PostgreSql;

namespace EventManager.API.IntegrationTests.Application.Services.EventService;

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
}