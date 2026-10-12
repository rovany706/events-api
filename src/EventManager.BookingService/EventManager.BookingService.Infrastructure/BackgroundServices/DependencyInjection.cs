using Microsoft.Extensions.DependencyInjection;

namespace EventManager.BookingService.Infrastructure.BackgroundServices;

internal static class DependencyInjection
{
    internal static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<BookingConfirmationBackgroundService>();
        services.AddHostedService<BookingCancellationBackgroundService>();
        services.AddHostedService<BookingRejectedEventConsumer>();
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}