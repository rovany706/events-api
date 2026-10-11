using EventManager.UserService.Application.Abstractions.Security;
using EventManager.UserService.Infrastructure.Persistence;
using EventManager.UserService.Infrastructure.Security;

using Microsoft.Extensions.DependencyInjection;

namespace EventManager.UserService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string dbConnectionString)
    {
        services.AddPersistence(dbConnectionString);
        services.AddTransient<IUserJwtTokenGenerator, UserJwtTokenGenerator>();

        return services;
    }
}