using EventManager.Application.Abstractions.Services;
using EventManager.Infrastructure.Services.BookingService;
using EventManager.Infrastructure.Services.EventService;

using Microsoft.Extensions.DependencyInjection;

namespace EventManager.Infrastructure.Services;

internal static class DependencyInjection
{
    internal static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IEventService, EventServiceImpl>();
        services.AddScoped<IBookingService, BookingServiceImpl>();
        
        return services;
    }
}