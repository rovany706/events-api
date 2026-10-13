using EventManager.EventService.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManager.EventService.Infrastructure.Persistence.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("events");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .IsRequired();

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.StartAt)
            .IsRequired();

        builder.Property(e => e.EndAt)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(255);

        builder.Property(e => e.TotalSeats)
            .IsRequired();

        builder.Property(e => e.AvailableSeats)
            .IsRequired();

        builder.Property(b => b.RowVersion)
            .IsRowVersion();
        
        builder.Ignore(e => e.HasStarted);
    }
}