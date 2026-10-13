using EventManager.BookingService.Application.Abstractions.Persistence.Repositories;
using EventManager.BookingService.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace EventManager.BookingService.Infrastructure.Persistence.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly BookingDbContext _dbContext;

    public BookingRepository(BookingDbContext dbContext)
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
    public async Task<IReadOnlyList<Booking>> GetPendingCancellationBookingsAsync(CancellationToken ct)
    {
        return await _dbContext.Bookings
            .Where(b => b.Status == BookingStatus.PendingCancellation)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public Task<Booking?> GetBookingByIdAsync(int id, CancellationToken ct)
    {
        return _dbContext.Bookings.FirstOrDefaultAsync(b => b.Id == id, ct);
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

    /// <inheritdoc />
    public Task<int> GetActiveBookingCountForUserAsync(int userId, CancellationToken cancellationToken)
    {
        return _dbContext.Bookings
            .AsNoTracking()
            .CountAsync(b =>
                    b.UserId == userId
                    // TODO хз как переделать, Event больше нет
                    // && DateTime.UtcNow <= b.Event.StartAt 
                    && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending),
                cancellationToken);
    }
}