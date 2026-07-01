using EventManager.API.Models.Entities;

namespace EventManager.API.Tests.Models;

internal static class EventTestDataGenerator
{
    public static readonly DateTime Now = new DateTime(2026, 6, 7, 12, 0, 0);
    
    public static IEnumerable<Event> GetTestEvents()
    {
        return new List<Event>
        {
            // Уже законченные мероприятия 
            new()
            {
                Id = 1,
                Title = "Past Conference",
                Description = "Already over",
                StartAt = Now.AddDays(-10),
                EndAt = Now.AddDays(-8)
            },
            new()
            {
                Id = 2,
                Title = "Old Conference",
                Description = null,
                StartAt = Now.AddDays(-5),
                EndAt = Now.AddDays(-4)
            },
            new()
            {
                Id = 3,
                Title = "Last Week Meetup",
                Description = "Ended recently",
                StartAt = Now.AddDays(-3),
                EndAt = Now.AddDays(-2)
            },

            // Начавшиеся мероприятия (StartAt <= Now < EndAt)
            new()
            {
                Id = 4,
                Title = "Running Festival",
                Description = "Ongoing right Now",
                StartAt = Now.AddDays(-1),
                EndAt = Now.AddDays(1)
            },
            new()
            {
                Id = 5,
                Title = "Week-Long Expo",
                Description = null,
                StartAt = Now.AddDays(-2),
                EndAt = Now.AddDays(5)
            },
            new()
            {
                Id = 6,
                Title = "Multi-Day Summit 2026",
                Description = "Started yesterday",
                StartAt = Now.AddHours(-6),
                EndAt = Now.AddHours(18)
            },


            // Мероприятие, начинающееся сейчас
            new()
            {
                Id = 7,
                Title = "Starting Now",
                Description = "Edge: starts at Now",
                StartAt = Now,
                EndAt = Now.AddHours(4)
            },


            // Будущие мероприятия (StartAt > Now)
            new()
            {
                Id = 8,
                Title = "Tomorrow Talk",
                Description = null,
                StartAt = Now.AddDays(1),
                EndAt = Now.AddDays(1).AddHours(3)
            },
            new()
            {
                Id = 9,
                Title = "Upcoming Hackathon",
                Description = "Next weekend",
                StartAt = Now.AddDays(3),
                EndAt = Now.AddDays(4)
            },
            new()
            {
                Id = 10,
                Title = "Tech Symposium",
                Description = null,
                StartAt = Now.AddDays(7),
                EndAt = Now.AddDays(9)
            },
            new()
            {
                Id = 11,
                Title = "Summer Fair 2026",
                Description = "Family friendly",
                StartAt = Now.AddDays(14),
                EndAt = Now.AddDays(14).AddHours(8)
            },
            new()
            {
                Id = 12,
                Title = "Annual Gala",
                Description = null,
                StartAt = Now.AddDays(30),
                EndAt = Now.AddDays(30).AddHours(5)
            },

            // Сегодняшние мероприятия, 1 в прошлом, 1 в будущем
            new()
            {
                Id = 13,
                Title = "Flash Meetup",
                Description = "1 hour, past",
                StartAt = Now.AddHours(-2),
                EndAt = Now.AddHours(-1)
            },
            new()
            {
                Id = 14,
                Title = "Quick Briefing",
                Description = "1 hour, future",
                StartAt = Now.AddHours(1),
                EndAt = Now.AddHours(2)
            },


            // Заканчивающееся прямо сейчас
            new()
            {
                Id = 15,
                Title = "Ending Now",
                Description = "Edge: ends at Now",
                StartAt = Now.AddDays(-1),
                EndAt = Now
            },
        };
    }
}