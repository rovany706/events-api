using EventManager.Domain.Exceptions;

namespace EventManager.Domain.Entities;

/// <summary>
/// Бронирование мероприятия
/// </summary>
public class Booking
{
    private Booking() { }

    private Booking(int eventId, int userId)
    {
        EventId = eventId;
        Status = BookingStatus.Pending;
        UserId = userId;
    }

    public static Booking CreateInstance(int eventId, int userId)
    {
        return new Booking(eventId, userId);
    }
    
    /// <summary>
    /// Уникальный идентификатор брони
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Идентификатор события, к которому относится бронь
    /// </summary>
    public int EventId { get; private set; }

    /// <summary>
    /// Текущий статус брони
    /// </summary>
    public BookingStatus Status { get; private set; }

    /// <summary>
    /// Дата и время создания брони
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Дата и время обработки брони
    /// </summary>
    public DateTime? ProcessedAt { get; private set; }

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public int UserId { get; private set; }

    /// <summary>
    /// Мероприятие
    /// </summary>
    public Event? Event { get; set; }

    /// <summary>
    /// Пользователь
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Подтвердить бронь
    /// </summary>
    public void Confirm()
    {
        Status = BookingStatus.Confirmed;
        ProcessedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Отклонить бронь
    /// </summary>
    public void Reject()
    {
        Status = BookingStatus.Rejected;
        ProcessedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == BookingStatus.Cancelled)
        {
            throw new BookingAlreadyCancelledException();
        }
        
        Status = BookingStatus.Cancelled;
        ProcessedAt = DateTime.UtcNow;
    }
}