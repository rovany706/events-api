using EventManager.Domain.Entities.Bookings;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManager.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .IsRequired();

        builder.Property(b => b.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<string>();
        
        builder.HasOne(b => b.Event)
            .WithMany(e => e.Bookings)
            .HasForeignKey(b => b.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}