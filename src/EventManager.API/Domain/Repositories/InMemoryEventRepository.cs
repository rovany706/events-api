using EventManager.API.Domain.Interfaces;
using EventManager.API.Models.Entities;

namespace EventManager.API.Domain.Repositories;

/// <summary>
/// Репозиторий мероприятий
/// </summary>
public class InMemoryEventRepository : IEventRepository
{
    private static readonly List<Event> _events = SeedEvents();
    private int _nextId = 1;

    private static List<Event> SeedEvents()
    {
        return
        [
            new() {
                Id = 1,
                Title = "Tech Workshop 2026",
                Description = "Annual gathering of tech leaders discussing AI, cloud computing, and the future of software development.",
                StartAt = new DateTime(2026, 3, 10, 9, 0, 0),
                EndAt = new DateTime(2026, 3, 12, 18, 0, 0),
                TotalSeats = 100
            },
            new() {
                Id = 2,
                Title = "Berlin Marathon",
                Description = "One of the six World Marathon Majors, running through the heart of Berlin.",
                StartAt = new DateTime(2026, 9, 27, 8, 30, 0),
                EndAt = new DateTime(2026, 9, 27, 16, 0, 0),
                TotalSeats = 1000
            },
            new() {
                Id = 3,
                Title = "Startup Pitch Night",
                Description = "Early-stage founders pitch their ideas to a panel of investors and industry experts.",
                StartAt = new DateTime(2026, 4, 15, 18, 0, 0),
                EndAt = new DateTime(2026, 4, 15, 21, 30, 0),
                TotalSeats = 150
            },
            new() {
                Id = 4,
                Title = "Jazz in the Park",
                Description = null,
                StartAt = new DateTime(2026, 7, 4, 17, 0, 0),
                EndAt = new DateTime(2026, 7, 4, 22, 0, 0),
                TotalSeats = 50
            },
            new() {
                Id = 5,
                Title = "Annual Charity Gala",
                Description = "Black-tie fundraising dinner supporting local children's hospitals.",
                StartAt = new DateTime(2026, 11, 21, 19, 0, 0),
                EndAt = new DateTime(2026, 11, 21, 23, 59, 0),
                TotalSeats = 250
            },
            new() {
                Id = 6,
                Title = "Photography Workshop",
                Description = "Hands-on workshop covering portrait, landscape, and street photography techniques.",
                StartAt = new DateTime(2026, 5, 8, 10, 0, 0),
                EndAt = new DateTime(2026, 5, 8, 16, 0, 0),
                TotalSeats = 30
            },
            new() {
                Id = 7,
                Title = "Food & Wine Festival",
                Description = "Three-day celebration of local cuisine, international wines, and live cooking demonstrations.",
                StartAt = new DateTime(2026, 8, 14, 11, 0, 0),
                EndAt = new DateTime(2026, 8, 16, 20, 0, 0),
                TotalSeats = 5000
            },
            new() {
                Id = 8,
                Title = "C# Advanced Workshop",
                Description = "Deep dive into .NET 9, performance tuning, and modern C# patterns for senior developers.",
                StartAt = new DateTime(2026, 6, 3, 9, 0, 0),
                EndAt = new DateTime(2026, 6, 3, 17, 0, 0),
                TotalSeats = 50
            },
            new() {
                Id = 9,
                Title = "Community Clean-Up Day",
                Description = null,
                StartAt = new DateTime(2026, 4, 22, 8, 0, 0),
                EndAt = new DateTime(2026, 4, 22, 13, 0, 0),
                TotalSeats = 20
            },
            new() {
                Id = 10,
                Title = "Book Fair 2026",
                Description = "Over 200 publishers and authors gathering for readings, signings, and panel discussions.",
                StartAt = new DateTime(2026, 10, 1, 9, 0, 0),
                EndAt = new DateTime(2026, 10, 5, 19, 0, 0),
                TotalSeats = 12000
            },
            new() {
                Id = 11,
                Title = "Product Design Meetup",
                Description = "Monthly UX/UI meetup featuring lightning talks and portfolio reviews.",
                StartAt = new DateTime(2026, 6, 18, 18, 30, 0),
                EndAt = new DateTime(2026, 6, 18, 21, 0, 0),
                TotalSeats = 80
            },
            new() {
                Id = 12,
                Title = "Open Air Cinema Night",
                Description = "Classic films screened outdoors at the city park. Bring your own blanket.",
                StartAt = new DateTime(2026, 7, 25, 20, 30, 0),
                EndAt = new DateTime(2026, 7, 25, 23, 30, 0),
                TotalSeats = 600
            },
            new() {
                Id = 13,
                Title = "Hackathon: Climate Tech",
                Description = "48-hour hackathon focused on building software solutions for climate and sustainability challenges.",
                StartAt = new DateTime(2026, 9, 5, 9, 0, 0),
                EndAt = new DateTime(2026, 9, 7, 9, 0, 0),
                TotalSeats = 250
            },
            new() {
                Id = 14,
                Title = "Yoga & Mindfulness Retreat",
                Description = null,
                StartAt = new DateTime(2026, 5, 22, 7, 0, 0),
                EndAt = new DateTime(2026, 5, 24, 17, 0, 0),
                TotalSeats = 5
            },
            new() {
                Id = 15,
                Title = "New Year's Eve Concert",
                Description = "Live orchestral performance and countdown celebration at the city concert hall.",
                StartAt = new DateTime(2026, 12, 31, 20, 0, 0),
                EndAt = new DateTime(2027, 1, 1, 1, 0, 0),
                TotalSeats = 2000
            }
        ];
    }

    /// <inheritdoc />
    public Event? GetEventById(int id)
    {
        return _events.FirstOrDefault(x => x.Id == id);
    }

    /// <inheritdoc />
    public IEnumerable<Event> GetEvents()
    {
        return _events;
    }

    /// <inheritdoc />
    public int AddEvent(Event eventToAdd)
    {
        eventToAdd.Id = _nextId++;
        _events.Add(eventToAdd);

        return eventToAdd.Id;
    }

    /// <inheritdoc />
    public void UpdateEvent(Event updatedEvent)
    {
        var eventIndex = _events.FindIndex(x => x.Id == updatedEvent.Id);
        _events[eventIndex] = updatedEvent;
    }

    /// <inheritdoc />
    public bool RemoveEvent(Event eventToRemove)
    {
        return _events.Remove(eventToRemove);
    }
}
