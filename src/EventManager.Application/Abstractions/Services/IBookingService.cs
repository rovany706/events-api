using EventManager.Application.Common.Results;
using EventManager.Domain.Entities.Bookings;

namespace EventManager.Application.Abstractions.Services;

/// <summary>
/// Интерфейс сервиса бронирования
/// </summary>
public interface IBookingService
{
    /// <summary>
    /// Создание брони для указанного события
    /// </summary>
    /// <param name="eventId">Идентификатор мероприятия</param>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Созданное бронирование</returns>
    Task<Result<Booking?>> CreateBookingAsync(int eventId, int userId, CancellationToken cancellationToken);

    /// <summary>
    /// Получение брони по идентификатору
    /// </summary>
    /// <param name="bookingId">Идентификатор брони</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Бронь</returns>
    Task<Result<Booking?>> GetBookingByIdAsync(int bookingId, CancellationToken cancellationToken);

    /// <summary>
    /// Отмена брони
    /// </summary>
    /// <param name="bookingId">Идентификатор брони</param>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результат операции</returns>
    Task<Result> CancelBookingAsync(int bookingId, int userId, CancellationToken cancellationToken);
}