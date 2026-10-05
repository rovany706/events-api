using Microsoft.Extensions.DependencyInjection;

namespace EventManager.Application.BackgroundServices;

internal static class DependencyInjection
{
    internal static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<BookingProcessorService>();

        return services;
    }
}