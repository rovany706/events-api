using EventManager.EventService.Application.Abstractions.Services;
using EventManager.EventService.Application.Services.EventService;

using Microsoft.Extensions.DependencyInjection;

namespace EventManager.EventService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IEventService, EventServiceImpl>();

        return services;
    }
}