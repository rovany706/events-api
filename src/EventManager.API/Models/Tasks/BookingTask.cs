namespace EventManager.API.Models.Tasks;

/// <summary>
/// Задача бронирования
/// </summary>
/// <param name="BookingId">Идентификатор брониорвания</param>
/// <param name="CreatedAt">Дата создания задачи</param>
public record BookingTask(int BookingId, DateTime CreatedAt);