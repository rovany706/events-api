using EventManager.BookingService.Application.Common.Messaging;
using EventManager.BookingService.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace EventManager.BookingService.Infrastructure.Persistence;

public class BookingDbContext : DbContext
{
    /// <inheritdoc />
    public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options)
    {
    }

    public DbSet<Booking> Bookings => Set<Booking>();
    
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    
    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingDbContext).Assembly);
    }
}