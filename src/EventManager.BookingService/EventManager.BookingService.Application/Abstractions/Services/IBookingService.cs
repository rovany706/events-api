using EventManager.BookingService.Application.Common.Results;
using EventManager.BookingService.Domain.Entities;

namespace EventManager.BookingService.Application.Abstractions.Services;

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
    Task<Result<Booking?>> CreateBookingAsync(int eventId, int userId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Получение брони по идентификатору
    /// </summary>
    /// <param name="bookingId">Идентификатор брони</param>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="userRole">Роль пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Бронь</returns>
    Task<Result<Booking?>> GetBookingByIdAsync(int bookingId, int userId, string userRole,
        CancellationToken cancellationToken);

    /// <summary>
    /// Отмена брони
    /// </summary>
    /// <param name="bookingId">Идентификатор брони</param>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="userRole">Роль пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результат операции</returns>
    Task<Result> CancelBookingAsync(int bookingId, int userId, string userRole, CancellationToken cancellationToken);
}