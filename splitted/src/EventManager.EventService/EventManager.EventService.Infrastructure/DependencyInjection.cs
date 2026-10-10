using EventManager.EventService.Infrastructure.Persistence;

using Microsoft.Extensions.DependencyInjection;

namespace EventManager.EventService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string dbConnectionString)
    {
        services.AddPersistence(dbConnectionString);

        return services;
    }
}