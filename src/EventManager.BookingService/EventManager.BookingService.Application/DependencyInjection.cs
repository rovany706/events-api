using EventManager.BookingService.Application.Abstractions.Services;
using EventManager.BookingService.Application.Services.BookingService;

using Microsoft.Extensions.DependencyInjection;

namespace EventManager.BookingService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IBookingService, BookingServiceImpl>();

        return services;
    }
}