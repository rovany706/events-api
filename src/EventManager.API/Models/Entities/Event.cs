using System.ComponentModel.DataAnnotations;

namespace EventManager.API.Models.Entities;

/// <summary>
/// Мероприятие
/// </summary>
public class Event
{
    private const int UndefinedId = 0;
    private int _availableSeats;

    private Event()
    {
        Title = null!;
    }

    private Event(int id, string title, string? description, DateTime startAt, DateTime endAt, int totalSeats)
    {
        Id = id; // ?
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
        TotalSeats = totalSeats;
        AvailableSeats = totalSeats;
    }

    public static Event CreateInstance(int id, string title, string? description, DateTime startAt, DateTime endAt,
        int totalSeats)
    {
        return new Event(id, title, description, startAt, endAt, totalSeats);
    }
    
    public static Event CreateInstance(string title, string? description, DateTime startAt, DateTime endAt,
        int totalSeats)
    {
        return CreateInstance(UndefinedId, title, description, startAt, endAt, totalSeats);
    }
    
    /// <summary>
    /// Идентификатор мероприятия
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Название мероприятия
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Описание мероприятия
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Дата начала мероприятия
    /// </summary>
    public DateTime StartAt { get; private set; }

    /// <summary>
    /// Дата конца мероприятия
    /// </summary>
    public DateTime EndAt { get; private set; }

    /// <summary>
    /// Общее количество мест на мероприятии
    /// </summary>
    public int TotalSeats { get; private set; }

    /// <summary>
    /// Текущее количество свободных мест
    /// </summary>
    public int AvailableSeats
    {
        get => _availableSeats;
        private set => _availableSeats = value;
    }

    /// <summary>
    /// Бронирования
    /// </summary>
    public List<Booking> Bookings { get; private set; }

    public void Update(string title, string? description, DateTime startAt, DateTime endAt)
    {
        ThrowIfNotValid(title, startAt, endAt, TotalSeats);

        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
    }

    /// <summary>
    /// Забронировать места
    /// </summary>
    /// <param name="count">Количество мест</param>
    /// <returns>true - резервирование успешно, false - мест для резервирования нет</returns>
    /// <exception cref="ArgumentOutOfRangeException">Количество мест для резервирования меньше или равно 0</exception>
    public bool TryReserveSeats(int count = 1)
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be positive.");
        }

        int current, updated;
        do
        {
            current = _availableSeats;

            if (current < count)
            {
                return false;
            }

            updated = current - count;
        } while (Interlocked.CompareExchange(ref _availableSeats, updated, current) != current);

        return true;
    }

    /// <summary>
    /// Освободить забронированные места
    /// </summary>
    /// <param name="count">Количество мест</param>
    /// <exception cref="ArgumentOutOfRangeException">Количество мест для освобождения больше общего количества мест.</exception>
    public void ReleaseSeats(int count = 1)
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be positive.");
        }

        if (count > TotalSeats)
        {
            throw new ArgumentOutOfRangeException(nameof(count),
                "Count must be less or equal to the total seat count.");
        }

        Interlocked.Add(ref _availableSeats, count);
    }

    private static void ThrowIfNotValid(string title, DateTime startAt, DateTime endAt, int totalSeats)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ValidationException("Event title is empty");
        }

        if (startAt > endAt)
        {
            throw new ValidationException("Event start date is greater than event end date");
        }

        if (totalSeats <= 0)
        {
            throw new ValidationException("Total seat count must be positive");
        }
    }
}