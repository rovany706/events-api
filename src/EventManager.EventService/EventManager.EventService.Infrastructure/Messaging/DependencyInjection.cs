using Confluent.Kafka;

using EventManager.EventService.Application.Abstractions.Messaging;

using Microsoft.Extensions.DependencyInjection;

namespace EventManager.EventService.Infrastructure.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, string bootstrapServers)
    {
        services.AddSingleton<IProducer<string, string>>(_ =>
            new ProducerBuilder<string, string>(new ProducerConfig { BootstrapServers = bootstrapServers }).Build());

        services.AddSingleton<IMessageBroker, KafkaMessageBroker>();
        return services;
    }
}