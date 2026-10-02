using EventManager.Application.Abstractions.Persistence.Repositories;
using EventManager.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventManager.Infrastructure.Persistence;

internal static class DependencyInjection
{
    internal static IServiceCollection AddPersistence(this IServiceCollection services, string dbConnectionString)
    {
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(dbConnectionString));

        return services;
    }
}