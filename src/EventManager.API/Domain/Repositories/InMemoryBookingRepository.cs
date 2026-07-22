using EventManager.API.Domain.Interfaces;
using EventManager.API.Models.Entities;

namespace EventManager.API.Domain.Repositories;

/// <summary>
/// Репозиторий бронирований
/// </summary>
public class InMemoryBookingRepository : IBookingRepository
{
    private static readonly List<Booking> _bookings = [];
    private int _currentId = 0;

    /// <inheritdoc />
    public IEnumerable<Booking> GetBookings()
    {
        return _bookings;
    }
    
    /// <inheritdoc />
    public Booking? GetBookingById(int id)
    {
        return _bookings.FirstOrDefault(x => x.Id == id);
    }

    /// <inheritdoc />
    public int AddBooking(Booking bookingToAdd)
    {
        var id = Interlocked.Increment(ref _currentId);
        bookingToAdd.Id = id;
        _bookings.Add(bookingToAdd);

        return bookingToAdd.Id;
    }

    /// <inheritdoc />
    public void UpdateBooking(Booking updatedBooking)
    {
        var bookingIndex = _bookings.FindIndex(x => x.Id == updatedBooking.Id);
        _bookings[bookingIndex] = updatedBooking;
    }

    /// <inheritdoc />
    public bool RemoveBooking(Booking bookingToRemove)
    {
        return _bookings.Remove(bookingToRemove);
    }

    /// <inheritdoc />
    public IEnumerable<Booking> GetPendingBookings()
    {
        return _bookings.Where(b => b.Status == BookingStatus.Pending);
    }
}