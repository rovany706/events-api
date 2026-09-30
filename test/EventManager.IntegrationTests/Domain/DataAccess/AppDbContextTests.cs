using EventManager.Infrastructure.Persistence;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Testcontainers.PostgreSql;

namespace EventManager.IntegrationTests.Domain.DataAccess;

public class AppDbContextTests : IAsyncLifetime
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
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        await using var context = new AppDbContext(options);

        var hasPendingModelChanges = context.Database.HasPendingModelChanges();

        hasPendingModelChanges.Should().BeFalse();
    }
}