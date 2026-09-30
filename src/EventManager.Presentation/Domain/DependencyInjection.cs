using EventManager.Application.Abstractions.Persistence.Repositories;
using EventManager.Presentation.Domain.DataAccess;
using EventManager.Presentation.Domain.Repositories;

using Microsoft.EntityFrameworkCore;

namespace EventManager.Presentation.Domain;

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