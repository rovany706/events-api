using EventManager.Infrastructure.BackgroundServices;
using EventManager.Infrastructure.Persistence;
using EventManager.Infrastructure.Services;

using Microsoft.Extensions.DependencyInjection;

namespace EventManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string dbConnectionString)
    {
        services.AddPersistence(dbConnectionString);
        services.AddServices();
        services.AddBackgroundServices();
        
        return services;
    }
}