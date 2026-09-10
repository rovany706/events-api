using EventManager.API.Domain.DataAccess;

using Microsoft.EntityFrameworkCore;

namespace EventManager.API.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomain(this IServiceCollection services, string dbConnectionString)
    {
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(dbConnectionString));
        
        return services;
    }
}