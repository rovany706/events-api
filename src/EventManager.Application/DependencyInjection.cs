using EventManager.Application.Abstractions.Security;
using EventManager.Application.Abstractions.Services;
using EventManager.Application.BackgroundServices;
using EventManager.Application.Security;
using EventManager.Application.Services.BookingService;
using EventManager.Application.Services.EventService;
using EventManager.Application.Services.UserService;

using Microsoft.Extensions.DependencyInjection;

namespace EventManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddBackgroundServices();
        services.AddTransient<IPasswordHasher, SHA256PasswordHasher>();
        
        services.AddScoped<IEventService, EventServiceImpl>();
        services.AddScoped<IBookingService, BookingServiceImpl>();
        services.AddScoped<IUserService, UserServiceImpl>();

        return services;
    }
}