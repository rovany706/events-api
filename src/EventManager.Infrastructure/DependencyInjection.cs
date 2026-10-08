using EventManager.Application.Abstractions.Security;
using EventManager.Infrastructure.Persistence;
using EventManager.Infrastructure.Security;

using Microsoft.Extensions.DependencyInjection;

namespace EventManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string dbConnectionString)
    {
        services.AddPersistence(dbConnectionString);
        services.AddTransient<IUserJwtTokenGenerator, UserJwtTokenGenerator>();

        return services;
    }
}