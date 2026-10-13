using EventManager.UserService.Application.Abstractions.Security;
using EventManager.UserService.Application.Abstractions.Services;
using EventManager.UserService.Application.Security;
using EventManager.UserService.Application.Services.UserService;

using Microsoft.Extensions.DependencyInjection;

namespace EventManager.UserService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddTransient<IPasswordHasher, SHA256PasswordHasher>();
        services.AddScoped<IUserService, UserServiceImpl>();

        return services;
    }
}