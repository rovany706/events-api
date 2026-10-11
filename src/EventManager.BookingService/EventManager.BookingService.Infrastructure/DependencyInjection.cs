using EventManager.BookingService.Infrastructure.Messaging;
using EventManager.BookingService.Infrastructure.Persistence;

using Microsoft.Extensions.DependencyInjection;

namespace EventManager.BookingService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string dbConnectionString,
        string bootstrapServers)
    {
        services.AddPersistence(dbConnectionString);
        services.AddMessaging(bootstrapServers);
        
        return services;
    }
}