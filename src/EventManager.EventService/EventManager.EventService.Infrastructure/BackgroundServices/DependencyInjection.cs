using Microsoft.Extensions.DependencyInjection;

namespace EventManager.EventService.Infrastructure.BackgroundServices;

public static class DependencyInjection
{
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<KafkaTopicInitializer>();
        services.AddHostedService<BookingConfirmationEventConsumer>();
        services.AddHostedService<BookingCancelledEventConsumer>();
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}