namespace EventManager.Contracts.Events;

/// <summary>
/// Событие отмены бронирования
/// </summary>
/// <param name="EventId">Идентификатор мероприятия</param>
/// <param name="BookingId">Идентификатор бронирования</param>
/// <param name="UserId">Идентификатор пользователя</param>
/// <param name="SeatCount">Количество мест</param>
public record BookingCancelledEvent(
    Guid MessageId,
    int EventId,
    int BookingId,
    int UserId,
    int SeatCount);