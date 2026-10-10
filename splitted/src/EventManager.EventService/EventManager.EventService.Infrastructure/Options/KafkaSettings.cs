using System.ComponentModel.DataAnnotations;

namespace EventManager.EventService.Infrastructure.Options;

public class KafkaSettings
{
    [Required]
    public string BootstrapServers { get; set; }
    
    [Required]
    public string ConsumerGroup { get; set; }
}