namespace EventManager.Contracts.Events;

/// <summary>
/// Событие подтверждения бронирования
/// </summary>
/// <param name="EventId">Идентификатор мероприятия</param>
/// <param name="BookingId">Идентификатор бронирования</param>
/// <param name="UserId">Идентификатор пользователя</param>
/// <param name="SeatCount">Количество мест</param>
/// <param name="ProcessedAt">Дата и время обработки бронирования</param>
public record BookingConfirmedEvent(
    Guid MessageId,
    int EventId,
    int BookingId,
    int UserId,
    int SeatCount,
    DateTime ProcessedAt);