using EventManager.Domain.Entities;
using EventManager.Domain.Entities.Bookings;

using Microsoft.EntityFrameworkCore;

namespace EventManager.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    /// <inheritdoc />
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Event> Events => Set<Event>();

    public DbSet<Booking> Bookings => Set<Booking>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}