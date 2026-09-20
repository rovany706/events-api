using EventManager.API.Domain.DataAccess;
using EventManager.API.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventManager.API.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomain(this IServiceCollection services, string dbConnectionString)
    {
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(dbConnectionString));
        
        return services;
    }
}