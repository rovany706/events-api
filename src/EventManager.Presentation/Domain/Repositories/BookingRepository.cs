using EventManager.Application.Abstractions.Persistence.Repositories;
using EventManager.Domain.Entities;
using EventManager.Presentation.Domain.DataAccess;

using Microsoft.EntityFrameworkCore;

namespace EventManager.Presentation.Domain.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _dbContext;

    public BookingRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    /// <inheritdoc />
    public IQueryable<Booking> GetBookings()
    {
        return _dbContext.Bookings.AsQueryable().AsNoTracking();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Booking>> GetPendingBookingsAsync(CancellationToken ct)
    {
        return await _dbContext.Bookings
            .Where(b => b.Status == BookingStatus.Pending)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public Task<Booking?> GetBookingByIdAsync(int id, CancellationToken ct)
    {
        return _dbContext.Bookings.Include(b => b.Event).FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    /// <inheritdoc />
    public Task AddBookingAsync(Booking bookingToAdd, CancellationToken ct)
    {
        return _dbContext.Bookings.AddAsync(bookingToAdd, ct).AsTask();
    }

    /// <inheritdoc />
    public void RemoveBooking(Booking bookingToRemove)
    {
        _dbContext.Bookings.Remove(bookingToRemove);
    }

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken ct)
    {
        return _dbContext.SaveChangesAsync(ct);
    }
}