using Confluent.Kafka;
using Confluent.Kafka.Admin;

using EventManager.Contracts;
using EventManager.EventService.Infrastructure.Options;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventManager.EventService.Infrastructure.BackgroundServices;

public class KafkaTopicInitializer : BackgroundService
{
    private readonly KafkaSettings _kafkaSettings;
    private readonly ILogger<KafkaTopicInitializer> _logger;

    public KafkaTopicInitializer(IOptions<KafkaSettings> options, ILogger<KafkaTopicInitializer> logger)
    {
        _kafkaSettings = options.Value;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new AdminClientConfig { BootstrapServers = _kafkaSettings.BootstrapServers };
        using var adminClient = new AdminClientBuilder(config).Build();

        try
        {
            await adminClient.CreateTopicsAsync(
            [
                new TopicSpecification
                {
                    Name = KafkaTopics.BookingConfirmedTopicName, NumPartitions = 2, ReplicationFactor = 1
                },
                new TopicSpecification
                {
                    Name = KafkaTopics.BookingCancelledTopicName, NumPartitions = 2, ReplicationFactor = 1
                },
                new TopicSpecification
                {
                    Name = KafkaTopics.BookingRejectedTopicName, NumPartitions = 2, ReplicationFactor = 1
                }
            ]);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to create topics in Kafka.");
            return;
        }
        
        _logger.LogInformation("Created topics in Kafka.");
    }
}