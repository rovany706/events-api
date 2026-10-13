namespace EventManager.Contracts.Events;

/// <summary>
/// Событие отклонения бронирования
/// </summary>
/// <param name="EventId">Идентификатор мероприятия</param>
/// <param name="BookingId">Идентификатор бронирования</param>
/// <param name="UserId">Идентификатор пользователя</param>
public record BookingRejectedEvent(
    Guid MessageId,
    int EventId,
    int BookingId,
    int UserId);