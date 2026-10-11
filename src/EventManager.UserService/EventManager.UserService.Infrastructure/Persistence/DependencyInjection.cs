using EventManager.UserService.Application.Abstractions.Persistence.Repositories;
using EventManager.UserService.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventManager.UserService.Infrastructure.Persistence;

internal static class DependencyInjection
{
    internal static IServiceCollection AddPersistence(this IServiceCollection services, string dbConnectionString)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddDbContext<UserDbContext>(options => options.UseNpgsql(dbConnectionString));

        return services;
    }
}