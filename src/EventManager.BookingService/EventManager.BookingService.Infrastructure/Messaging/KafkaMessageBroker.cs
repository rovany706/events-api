using Confluent.Kafka;

using EventManager.BookingService.Application.Abstractions.Messaging;

using Microsoft.Extensions.Logging;

namespace EventManager.BookingService.Infrastructure.Messaging;

public class KafkaMessageBroker : IMessageBroker
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaMessageBroker> _logger;

    public KafkaMessageBroker(IProducer<string, string> producer,
        ILogger<KafkaMessageBroker> logger)
    {
        _producer = producer;
        _logger = logger;
    }
    
    public async Task PublishAsync(string topic, string key, string payload, CancellationToken cancellationToken)
    {
        var result = await _producer.ProduceAsync(topic,
            new Message<string, string>
            {
                Key = key, Value = payload
            }, cancellationToken);

        _logger.LogDebug("Published message: Topic = {Topic}, Partition = {Partition}, Offset = {Offset}",
            topic, result.Partition.Value, result.Offset.Value);
    }
}
