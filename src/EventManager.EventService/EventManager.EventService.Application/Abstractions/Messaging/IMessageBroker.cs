namespace EventManager.EventService.Application.Abstractions.Messaging;

/// <summary>
/// Интерфейс издателя событий
/// </summary>
public interface IMessageBroker
{
    /// <summary>
    /// Отправка сообщения
    /// </summary>
    /// <param name="topic">Топик</param>
    /// <param name="key">Ключ сообщения</param>
    /// <param name="payload">Тело сообщения</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns></returns>
    Task PublishAsync(string topic, string key, string payload, CancellationToken cancellationToken);
}