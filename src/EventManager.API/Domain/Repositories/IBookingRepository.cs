using EventManager.API.Models.Entities;

namespace EventManager.API.Domain.Repositories;

/// <summary>
/// Интерфейс репозитория бронирований
/// </summary>
public interface IBookingRepository
{
    /// <summary>
    /// Получить все бронирования
    /// </summary>
    IQueryable<Booking> GetBookings();

    /// <summary>
    /// Получить неподтвержденные бронирования
    /// </summary>
    /// <param name="ct">Токен отмены</param>
    Task<IReadOnlyList<Booking>> GetPendingBookingsAsync(CancellationToken ct);

    /// <summary>
    /// Получить бронирование по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор бронирования</param>
    /// <param name="ct">Токен отмены</param>
    Task<Booking?> GetBookingByIdAsync(int id, CancellationToken ct);

    /// <summary>
    /// Добавить бронирование
    /// </summary>
    /// <param name="bookingToAdd">Бронирование</param>
    /// <param name="ct">Токен отмены</param>
    Task AddBookingAsync(Booking bookingToAdd, CancellationToken ct);

    /// <summary>
    /// Удалить бронирование
    /// </summary>
    /// <param name="bookingToRemove">Бронирование</param>
    void RemoveBooking(Booking bookingToRemove);

    /// <summary>
    /// Сохранить изменения
    /// </summary>
    /// <param name="ct">Токен отмены</param>
    Task SaveChangesAsync(CancellationToken ct);
}