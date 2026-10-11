using Confluent.Kafka;
using Confluent.Kafka.Admin;

using EventManager.Contracts;

namespace EventManager.EventService.Infrastructure.Messaging;

public static class KafkaTopicInitializer
{
    public static async Task EnsureTopicsCreated(string bootstrapServers)
    {
        var config = new AdminClientConfig { BootstrapServers = bootstrapServers };
        using var adminClient = new AdminClientBuilder(config).Build();

        try
        {
            await adminClient.CreateTopicsAsync(
            [
                new TopicSpecification
                {
                    Name = KafkaTopics.BookingConfirmedTopicName, NumPartitions = 2, ReplicationFactor = 1
                }
            ]);
        }
        catch (CreateTopicsException e) when (e.Results.All(r =>
                                                  r.Error.Code is ErrorCode.TopicAlreadyExists or ErrorCode.NoError))
        {
        }
    }
}