using EventManager.EventService.Infrastructure.BackgroundServices;
using EventManager.EventService.Infrastructure.Messaging;
using EventManager.EventService.Infrastructure.Persistence;

using Microsoft.Extensions.DependencyInjection;

namespace EventManager.EventService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string dbConnectionString,
        string bootstrapServers)
    {
        services.AddBackgroundServices();
        services.AddMessaging(bootstrapServers);
        services.AddPersistence(dbConnectionString);

        return services;
    }
}