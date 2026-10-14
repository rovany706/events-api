namespace EventManager.EventService.Application.Abstractions.Caching;

public static class CacheTtlValues
{
    public static TimeSpan GetEventByIdTtl = TimeSpan.FromMinutes(5);
    
    public static TimeSpan GetTop10PopularEventsTtl = TimeSpan.FromHours(1);
}