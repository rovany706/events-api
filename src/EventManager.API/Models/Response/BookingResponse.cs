using EventManager.API.Models.Entities;

namespace EventManager.API.Models.Response;

/// <summary>
/// Бронирование
/// </summary>
public record BookingResponse
{
    /// <summary>
    /// Уникальный идентификатор брони
    /// </summary>
    public required int Id { get; set; }

    /// <summary>
    /// Идентификатор события, к которому относится бронь
    /// </summary>
    public required int EventId { get; init; }

    /// <summary>
    /// Текущий статус брони
    /// </summary>
    public required BookingStatus Status { get; set; }

    /// <summary>
    /// Дата и время создания брони
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Дата и время обработки брони
    /// </summary>
    public DateTime? ProcessedAt { get; set; }
}