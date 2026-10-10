using EventManager.Domain.Entities;
using EventManager.Domain.Entities.Bookings;
using EventManager.Domain.Entities.Users;

using Microsoft.EntityFrameworkCore;

namespace EventManager.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    /// <inheritdoc />
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Event> Events => Set<Event>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}