namespace EventManager.API.Models.Entities;

/// <summary>
/// Бронирование мероприятия
/// </summary>
public class Booking
{
    private Booking() { }

    private Booking(int eventId)
    {
        EventId = eventId;
        Status = BookingStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public static Booking CreateInstance(int eventId)
    {
        return new Booking(eventId);
    }
    
    /// <summary>
    /// Уникальный идентификатор брони
    /// </summary>
    public int Id { get; private set; }

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
    /// Мероприятие
    /// </summary>
    public Event Event { get; set; }

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
}