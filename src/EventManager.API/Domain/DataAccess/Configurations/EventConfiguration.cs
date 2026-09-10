using EventManager.API.Models.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManager.API.Domain.DataAccess.Configurations;

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

        builder.HasMany(e => e.Bookings)
            .WithOne(b => b.Event)
            .HasForeignKey(b => b.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(
            Event.CreateInstance(
                1,
                "Tech Workshop 2026",
                "Annual gathering of tech leaders discussing AI, cloud computing, and the future of software development.",
                DateTime.SpecifyKind(new DateTime(2026, 3, 10, 9, 0, 0), DateTimeKind.Utc),
                DateTime.SpecifyKind(new DateTime(2026, 3, 12, 18, 0, 0), DateTimeKind.Utc),
                3
            ),
            Event.CreateInstance(
                2,
                "Berlin Marathon",
                "One of the six World Marathon Majors, running through the heart of Berlin.",
                DateTime.SpecifyKind(new DateTime(2026, 9, 27, 8, 30, 0), DateTimeKind.Utc),
                DateTime.SpecifyKind(new DateTime(2026, 9, 27, 16, 0, 0), DateTimeKind.Utc),
                1000
            ),
            Event.CreateInstance(
                3,
                "Startup Pitch Night",
                "Early-stage founders pitch their ideas to a panel of investors and industry experts.",
                DateTime.SpecifyKind(new DateTime(2026, 4, 15, 18, 0, 0), DateTimeKind.Utc),
                DateTime.SpecifyKind(new DateTime(2026, 4, 15, 21, 30, 0), DateTimeKind.Utc),
                150
            ),
            Event.CreateInstance(
                4,
                "Jazz in the Park",
                null,
                DateTime.SpecifyKind(new DateTime(2026, 7, 4, 17, 0, 0), DateTimeKind.Utc),
                DateTime.SpecifyKind(new DateTime(2026, 7, 4, 22, 0, 0), DateTimeKind.Utc),
                50
            ),
            Event.CreateInstance(
                5,
                "Annual Charity Gala",
                "Black-tie fundraising dinner supporting local children's hospitals.",
                DateTime.SpecifyKind(new DateTime(2026, 11, 21, 19, 0, 0), DateTimeKind.Utc),
                DateTime.SpecifyKind(new DateTime(2026, 11, 21, 23, 59, 0), DateTimeKind.Utc),
                250
            ),
            Event.CreateInstance(
                6,
                "Photography Workshop",
                "Hands-on workshop covering portrait, landscape, and street photography techniques.",
                DateTime.SpecifyKind(new DateTime(2026, 5, 8, 10, 0, 0), DateTimeKind.Utc),
                DateTime.SpecifyKind(new DateTime(2026, 5, 8, 16, 0, 0), DateTimeKind.Utc),
                30
            ),
            Event.CreateInstance(
                7,
                "Food & Wine Festival",
                "Three-day celebration of local cuisine, international wines, and live cooking demonstrations.",
                DateTime.SpecifyKind(new DateTime(2026, 8, 14, 11, 0, 0), DateTimeKind.Utc),
                DateTime.SpecifyKind(new DateTime(2026, 8, 16, 20, 0, 0), DateTimeKind.Utc),
                5000
            ),
            Event.CreateInstance(
                8,
                "C# Advanced Workshop",
                "Deep dive into .NET 9, performance tuning, and modern C# patterns for senior developers.",
                DateTime.SpecifyKind(new DateTime(2026, 6, 3, 9, 0, 0), DateTimeKind.Utc),
                DateTime.SpecifyKind(new DateTime(2026, 6, 3, 17, 0, 0), DateTimeKind.Utc),
                50
            ),
            Event.CreateInstance(
                9,
                "Community Clean-Up Day",
                null,
                DateTime.SpecifyKind(new DateTime(2026, 4, 22, 8, 0, 0), DateTimeKind.Utc),
                DateTime.SpecifyKind(new DateTime(2026, 4, 22, 13, 0, 0), DateTimeKind.Utc),
                20
            ),
            Event.CreateInstance(
                10,
                "Book Fair 2026",
                "Over 200 publishers and authors gathering for readings, signings, and panel discussions.",
                DateTime.SpecifyKind(new DateTime(2026, 10, 1, 9, 0, 0), DateTimeKind.Utc),
                DateTime.SpecifyKind(new DateTime(2026, 10, 5, 19, 0, 0), DateTimeKind.Utc),
                12000
            ),
            Event.CreateInstance(
                11,
                "Product Design Meetup",
                "Monthly UX/UI meetup featuring lightning talks and portfolio reviews.",
                DateTime.SpecifyKind(new DateTime(2026, 6, 18, 18, 30, 0), DateTimeKind.Utc),
                DateTime.SpecifyKind(new DateTime(2026, 6, 18, 21, 0, 0), DateTimeKind.Utc),
                80
            ),
            Event.CreateInstance(
                12,
                "Open Air Cinema Night",
                "Classic films screened outdoors at the city park. Bring your own blanket.",
                DateTime.SpecifyKind(new DateTime(2026, 7, 25, 20, 30, 0), DateTimeKind.Utc),
                DateTime.SpecifyKind(new DateTime(2026, 7, 25, 23, 30, 0), DateTimeKind.Utc),
                600
            ),
            Event.CreateInstance(
                13,
                "Hackathon: Climate Tech",
                "48-hour hackathon focused on building software solutions for climate and sustainability challenges.",
                DateTime.SpecifyKind(new DateTime(2026, 9, 5, 9, 0, 0), DateTimeKind.Utc),
                DateTime.SpecifyKind(new DateTime(2026, 9, 7, 9, 0, 0), DateTimeKind.Utc),
                250
            ),
            Event.CreateInstance(
                14,
                "Yoga & Mindfulness Retreat",
                null,
                DateTime.SpecifyKind(new DateTime(2026, 5, 22, 7, 0, 0), DateTimeKind.Utc),
                DateTime.SpecifyKind(new DateTime(2026, 5, 24, 17, 0, 0), DateTimeKind.Utc),
                5
            ),
            Event.CreateInstance(
                15,
                "New Year's Eve Concert",
                "Live orchestral performance and countdown celebration at the city concert hall.",
                DateTime.SpecifyKind(new DateTime(2026, 12, 31, 20, 0, 0), DateTimeKind.Utc),
                DateTime.SpecifyKind(new DateTime(2027, 1, 1, 1, 0, 0), DateTimeKind.Utc),
                2000
            )
        );
    }
}