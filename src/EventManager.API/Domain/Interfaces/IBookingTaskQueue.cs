using EventManager.API.Models.Tasks;

namespace EventManager.API.Domain.Interfaces;

/// <summary>
/// Очередь задач бронирования
/// </summary>
public interface IBookingTaskQueue
{
    /// <summary>
    /// Поставить задачу в очередь
    /// </summary>
    /// <param name="bookingTask">Задача</param>
    void Enqueue(BookingTask bookingTask);

    /// <summary>
    /// Получить задачу из очереди 
    /// </summary>
    /// <param name="bookingTask">Задача</param>
    bool TryDequeue(out BookingTask bookingTask);
}