using EventManager.API.Domain.DataAccess;
using EventManager.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventManager.API.Domain.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _dbContext;

    public BookingRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public IQueryable<Booking> GetBookings()
    {
        return _dbContext.Bookings.AsQueryable().AsNoTracking();
    }

    public async Task<IReadOnlyList<Booking>> GetPendingBookingsAsync(CancellationToken ct)
    {
        return await _dbContext.Bookings
            .Where(b => b.Status == BookingStatus.Pending)
            .ToListAsync(ct);
    }

    public Task<Booking?> GetBookingByIdAsync(int id, CancellationToken ct)
    {
        return _dbContext.Bookings.FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public Task AddBookingAsync(Booking bookingToAdd, CancellationToken ct)
    {
        return _dbContext.Bookings.AddAsync(bookingToAdd, ct).AsTask();
    }

    public void RemoveBooking(Booking bookingToRemove)
    {
        _dbContext.Bookings.Remove(bookingToRemove);
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        return _dbContext.SaveChangesAsync(ct);
    }
}