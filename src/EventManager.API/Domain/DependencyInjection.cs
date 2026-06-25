using EventManager.API.Domain.Interfaces;
using EventManager.API.Domain.Queues;
using EventManager.API.Domain.Repositories;

namespace EventManager.API.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddSingleton<IEventRepository, InMemoryEventRepository>();
        services.AddSingleton<IBookingRepository, InMemoryBookingRepository>();
        services.AddSingleton<IBookingTaskQueue, InMemoryBookingTaskQueue>();

        return services;
    }
}
