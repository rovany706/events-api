using System.ComponentModel.DataAnnotations;

namespace EventManager.EventService.Infrastructure.Options;

public class KafkaSettings
{
    [Required]
    public required string BootstrapServers { get; init; }
    
    [Required]
    public required string ConsumerGroup { get; init; }
}