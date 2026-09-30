using EventManager.Application.BackgroundServices;

using Microsoft.Extensions.DependencyInjection;

namespace EventManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // services.AddScoped<IEventService, EventServiceImpl>();
        // services.AddScoped<IBookingService, BookingServiceImpl>();

        services.AddHostedService<BookingProcessorService>();

        return services;
    }
}
