using EventManager.Application.Abstractions.Services;
using EventManager.Application.Services.BookingService;
using EventManager.Application.Services.EventService;

using Microsoft.Extensions.DependencyInjection;

namespace EventManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IEventService, EventServiceImpl>();
        services.AddScoped<IBookingService, BookingServiceImpl>();

        return services;
    }
}