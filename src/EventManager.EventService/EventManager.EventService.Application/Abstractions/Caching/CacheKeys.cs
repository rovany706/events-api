namespace EventManager.EventService.Application.Abstractions.Caching;

public static class CacheKeys
{
    public static string Event(int id)
    {
        return $"event:{id}";
    }

    public const string TopEvents = "events:top10";
}