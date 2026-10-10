using EventManager.EventService.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace EventManager.EventService.Infrastructure.Persistence;

public class EventDbContext : DbContext
{
    /// <inheritdoc />
    public EventDbContext(DbContextOptions<EventDbContext> options) : base(options) { }

    public DbSet<Event> Events => Set<Event>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventDbContext).Assembly);
    }
}