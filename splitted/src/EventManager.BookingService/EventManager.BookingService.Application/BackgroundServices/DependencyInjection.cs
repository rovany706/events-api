using Microsoft.Extensions.DependencyInjection;

namespace EventManager.BookingService.Application.BackgroundServices;

internal static class DependencyInjection
{
    internal static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<BookingProcessorService>();

        return services;
    }
}