using System.Text.Json;

using Confluent.Kafka;

using EventManager.BookingService.Application.Abstractions.Messaging;
using EventManager.Contracts;
using EventManager.Contracts.Events;

using Microsoft.Extensions.Logging;

namespace EventManager.BookingService.Infrastructure.Messaging;

public class BookingConfirmationPublisher : IBookingConfirmationPublisher
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<BookingConfirmationPublisher> _logger;

    public BookingConfirmationPublisher(IProducer<string, string> producer,
        ILogger<BookingConfirmationPublisher> logger)
    {
        _producer = producer;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task PublishAsync(BookingConfirmedEvent confirmation, CancellationToken cancellationToken)
    {
        var result = await _producer.ProduceAsync(KafkaTopics.BookingConfirmedTopicName,
            new Message<string, string>
            {
                Key = confirmation.EventId.ToString(), Value = JsonSerializer.Serialize(confirmation)
            }, cancellationToken);

        _logger.LogDebug(
            "Published booking confirmation event: EventId = {EventId}, Partition = {Partition}, Offset = {Offset}",
            confirmation.EventId, result.Partition.Value, result.Offset.Value);
    }
}