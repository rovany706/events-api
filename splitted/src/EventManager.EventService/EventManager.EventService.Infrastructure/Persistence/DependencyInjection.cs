using EventManager.EventService.Application.Abstractions.Persistence.Repositories;
using EventManager.EventService.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventManager.EventService.Infrastructure.Persistence;

internal static class DependencyInjection
{
    internal static IServiceCollection AddPersistence(this IServiceCollection services, string dbConnectionString)
    {
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddDbContext<EventDbContext>(options => options.UseNpgsql(dbConnectionString));

        return services;
    }
}