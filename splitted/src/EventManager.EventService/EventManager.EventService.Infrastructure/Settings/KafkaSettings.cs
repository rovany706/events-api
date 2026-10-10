using System.ComponentModel.DataAnnotations;

namespace EventManager.EventService.Infrastructure.Settings;

public class KafkaSettings
{
    [Required]
    public string BootstrapServers { get; set; }
    
    [Required]
    public string ConsumerGroup { get; set; }
}