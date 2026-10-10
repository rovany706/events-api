using EventManager.Contracts.Events;

namespace EventManager.BookingService.Application.Abstractions.Messaging;

/// <summary>
/// Интерфейс издателя событий подтверждения брони
/// </summary>
public interface IBookingConfirmationPublisher
{
    /// <summary>
    /// Опубликовать событие подтверждения брони
    /// </summary>
    /// <param name="confirmation">Событие с информацией</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns></returns>
    Task PublishAsync(BookingConfirmedEvent confirmation, CancellationToken cancellationToken);
}