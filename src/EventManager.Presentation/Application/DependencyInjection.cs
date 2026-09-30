using EventManager.Presentation.Application.BackgroundServices;
using EventManager.Presentation.Application.Services.BookingService;
using EventManager.Presentation.Application.Services.EventService;

namespace EventManager.Presentation.Application;

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
