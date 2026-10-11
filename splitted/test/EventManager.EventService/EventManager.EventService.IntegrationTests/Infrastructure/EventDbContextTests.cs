using EventManager.EventService.Infrastructure.Persistence;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Testcontainers.PostgreSql;

namespace EventManager.EventService.IntegrationTests.Infrastructure;

public class EventDbContextTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .Build();

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task DbContext_ShouldHaveNoPendingModelChanges()
    {
        var options = new DbContextOptionsBuilder<EventDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        await using var context = new EventDbContext(options);

        var hasPendingModelChanges = context.Database.HasPendingModelChanges();

        hasPendingModelChanges.Should().BeFalse();
    }
}