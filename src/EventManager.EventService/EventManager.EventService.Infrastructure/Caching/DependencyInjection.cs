using EventManager.EventService.Application.Abstractions.Caching;

using Microsoft.Extensions.DependencyInjection;

using StackExchange.Redis;

namespace EventManager.EventService.Infrastructure.Caching;

internal static class DependencyInjection
{
    internal static IServiceCollection AddCaching(this IServiceCollection services, string redisEndpoints)
    {
        var redisOptions = new ConfigurationOptions
        {
            EndPoints = { redisEndpoints }, ConnectTimeout = 3000, AbortOnConnectFail = false
        };

        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisOptions)
        );

        services.AddScoped<ICache, RedisCache>();

        return services;
    }
}