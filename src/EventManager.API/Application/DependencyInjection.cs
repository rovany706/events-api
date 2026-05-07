using EventManager.API.Application.Interfaces;
using EventManager.API.Application.Services;

namespace EventManager.API.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IEventService, EventService>();

        return services;
    }
}
