using EventManager.Domain.Entities.Bookings;
using EventManager.Domain.Exceptions;

namespace EventManager.Domain.Entities;

/// <summary>
/// Мероприятие
/// </summary>
public class Event
{
    private int _availableSeats;

    private Event()
    {
        Title = null!;
    }

    private Event(string title, string? description, DateTime startAt, DateTime endAt, int totalSeats)
    {
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
        TotalSeats = totalSeats;
        AvailableSeats = totalSeats;
    }

    public static Event CreateInstance(string title, string? description, DateTime startAt, DateTime endAt,
        int totalSeats)
    {
        ThrowIfNotValid(title, startAt, endAt, totalSeats);

        return new Event(title, description, startAt, endAt, totalSeats);
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

    public bool HasStarted => DateTime.UtcNow > StartAt;

    /// <summary>
    /// Бронирования
    /// </summary>
    public List<Booking> Bookings { get; private set; } = [];

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
    /// <exception cref="EventAlreadyStartedException">Мероприятие уже началось</exception>
    public bool TryReserveSeats(int count = 1)
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be positive.");
        }

        if (HasStarted)
        {
            throw new EventAlreadyStartedException();
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
            throw new EventValidationException("Event title is empty");
        }

        if (startAt > endAt)
        {
            throw new EventValidationException("Event start date is greater than event end date");
        }

        if (totalSeats <= 0)
        {
            throw new EventValidationException("Total seat count must be positive");
        }
    }
}