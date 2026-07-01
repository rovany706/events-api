using EventManager.API.Models.Entities;
using EventManager.API.Models.Results;

namespace EventManager.API.Application.Services.BookingService;

/// <summary>
/// Интерфейс сервиса бронирования
/// </summary>
public interface IBookingService
{
    /// <summary>
    /// Создание брони для указанного события
    /// </summary>
    /// <param name="eventId">Идентификатор мероприятия</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Созданное бронирование</returns>
    Task<Result<Booking?>> CreateBookingAsync(int eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получение брони по идентификатору
    /// </summary>
    /// <param name="bookingId">Идентификатор брони</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Бронь</returns>
    Task<Result<Booking?>> GetBookingByIdAsync(int bookingId, CancellationToken cancellationToken = default);
}