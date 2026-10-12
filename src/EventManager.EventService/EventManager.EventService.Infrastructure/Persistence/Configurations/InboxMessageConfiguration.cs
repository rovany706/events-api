using EventManager.EventService.Application.Common.Messaging;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManager.EventService.Infrastructure.Persistence.Configurations;

public class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("inbox_messages");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .IsRequired();

        builder.Property(m => m.ReceivedOn)
            .IsRequired();
    }
}