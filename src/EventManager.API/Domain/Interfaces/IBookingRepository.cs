using EventManager.API.Models.Entities;

namespace EventManager.API.Domain.Interfaces;

/// <summary>
/// Интерфейс репозитория бронирований
/// </summary>
public interface IBookingRepository
{
    /// <summary>
    /// Получить все бронирования
    /// </summary>
    IEnumerable<Booking> GetBookings();

    /// <summary>
    /// Получить неподтвержденные бронирования
    /// </summary>
    IEnumerable<Booking> GetPendingBookings();

    /// <summary>
    /// Получить бронирование по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор бронирования</param>
    Booking? GetBookingById(int id);

    /// <summary>
    /// Добавить бронирование
    /// </summary>
    /// <param name="bookingToAdd">Бронирование</param>
    /// <returns>Идентификатор, присвоенный бронированию</returns>
    int AddBooking(Booking bookingToAdd);

    /// <summary>
    /// Обновить бронирование
    /// </summary>
    /// <param name="updatedBooking">Обновленное бронирование</param>
    void UpdateBooking(Booking updatedBooking);

    /// <summary>
    /// Удалить бронирование
    /// </summary>
    /// <param name="bookingToRemove">Бронирование</param>
    /// <returns>false - бронирование не удалось удалить, иначе true</returns>
    bool RemoveBooking(Booking bookingToRemove);
}