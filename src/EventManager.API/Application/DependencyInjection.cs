using EventManager.API.Application.BackgroundServices;
using EventManager.API.Application.Services.BookingService;
using EventManager.API.Application.Services.EventService;

namespace EventManager.API.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IEventService, EventServiceImpl>();
        services.AddScoped<IBookingService, BookingServiceImpl>();

        services.AddHostedService<BookingProcessorService>();

        return services;
    }
}
