using EventManager.EventService.Application.Abstractions.Caching;
using EventManager.EventService.Application.Common.Results;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;

namespace EventManager.EventService.Infrastructure.Caching;

public class RedisCache : ICache
{
    private readonly ILogger<RedisCache> _logger;
    private readonly IDatabase _db;

    public RedisCache(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisCache> logger)
    {
        _logger = logger;
        _db = connectionMultiplexer.GetDatabase();
    }

    public async Task<Result<string>> GetByKeyAsync(string key)
    {
        try
        {
            var value = await _db.StringGetAsync(key);

            return value.HasValue
                ? Result<string>.Success(value.ToString())
                : Result<string>.Failure(Error.NotFound("Key not found"));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when retrieving value from Redis.");
            return Result<string>.Failure(Error.Failure("Error when retrieving value from Redis."));
        }
    }

    public async Task<Result> SetByKeyAsync(string key, string value, TimeSpan ttl)
    {
        try
        {
            await _db.StringSetAsync(key, value, ttl);

            return Result.Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when setting a value in Redis.");
            return Result.Failure(Error.Failure("Error when setting a value in Redis."));
        }
    }

    public async Task<Result> DeleteKeyAsync(string key)
    {
        try
        {
            await _db.KeyDeleteAsync(key);

            return Result.Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when deleting a key in Redis.");
            return Result.Failure(Error.Failure("Error when deleting a key in Redis."));
        }
    }
}