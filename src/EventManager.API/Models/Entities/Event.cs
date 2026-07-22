using System.ComponentModel.DataAnnotations;

namespace EventManager.API.Models.Entities;

/// <summary>
/// Мероприятие
/// </summary>
public record Event
{
    private int _availableSeats;

    /// <summary>
    /// Идентификатор мероприятия
    /// </summary>
    public required int Id { get; set; }

    /// <summary>
    /// Название мероприятия
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Описание мероприятия
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Дата начала мероприятия
    /// </summary>
    public required DateTime StartAt { get; init; }

    /// <summary>
    /// Дата конца мероприятия
    /// </summary>
    public required DateTime EndAt { get; init; }

    /// <summary>
    /// Общее количество мест на мероприятии
    /// </summary>
    public required int TotalSeats
    {
        get; init
        {
            if (value <= 0)
            {
                throw new ValidationException("Total seat count must be positive.");
            }

            field = value;
            _availableSeats = value;
        }
    }

    /// <summary>
    /// Текущее количество свободных мест
    /// </summary>
    public int AvailableSeats => _availableSeats;

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
    /// Освободить забронированые места
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
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be less or equal to the total seat count.");
        }

        Interlocked.Add(ref _availableSeats, count);
    }
}
