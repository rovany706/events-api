using EventManager.BookingService.Application.Common.Messaging;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManager.BookingService.Infrastructure.Persistence.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .IsRequired();

        builder.Property(m => m.Topic)
            .IsRequired();
        
        builder.Property(m => m.OccurredOn)
            .IsRequired();
        
        builder.Property(m => m.IsProcessed)
            .IsRequired();
        
        builder.Property(m => m.Payload)
            .IsRequired();
        
        builder.Property(m => m.Key)
            .IsRequired();
    }
}