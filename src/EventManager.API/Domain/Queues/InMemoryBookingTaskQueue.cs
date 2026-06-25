using System.Collections.Concurrent;

using EventManager.API.Domain.Interfaces;
using EventManager.API.Models.Tasks;

namespace EventManager.API.Domain.Queues;

/// <summary>
/// In-memory очередь задач бронирования
/// </summary>
public class InMemoryBookingTaskQueue : IBookingTaskQueue
{
    private readonly ConcurrentQueue<BookingTask> _queue = [];
    
    /// <inheritdoc />
    public void Enqueue(BookingTask bookingTask)
    {
        _queue.Enqueue(bookingTask);
    }

    /// <inheritdoc />
    public bool TryDequeue(out BookingTask bookingTask)
    {
        return _queue.TryDequeue(out bookingTask!);
    }
}