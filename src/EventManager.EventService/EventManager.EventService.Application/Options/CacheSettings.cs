using System.ComponentModel.DataAnnotations;

namespace EventManager.EventService.Application.Options;

public class CacheSettings
{
    [Required]
    public string Endpoints { get; init; }

    [Required]
    public TimeSpan EventTtl { get; init; }

    [Required]
    public TimeSpan TopEventsTtl { get; init; }
}