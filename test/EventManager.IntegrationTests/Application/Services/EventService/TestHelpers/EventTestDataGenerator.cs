using EventManager.Domain.Entities;

namespace EventManager.IntegrationTests.Application.Services.EventService.TestHelpers;

internal static class EventTestDataGenerator
{
    public static readonly DateTime Now = DateTime.SpecifyKind(new DateTime(2026, 6, 7, 12, 0, 0), DateTimeKind.Utc);

    public static IEnumerable<Event> GetTestEvents()
    {
        return new List<Event>
        {
            // Уже законченные мероприятия
            Event.CreateInstance("Past Conference",
                "Already over",
                Now.AddDays(-10),
                Now.AddDays(-8),
                1),
            Event.CreateInstance("Old Conference",
                null,
                Now.AddDays(-5),
                Now.AddDays(-4),
                2),
            Event.CreateInstance("Last Week Meetup",
                "Ended recently",
                Now.AddDays(-3),
                Now.AddDays(-2),
                3),

            // Начавшиеся мероприятия (StartAt <= Now < EndAt)
            Event.CreateInstance("Running Festival",
                "Ongoing right Now",
                Now.AddDays(-1),
                Now.AddDays(1),
                4),
            Event.CreateInstance("Week-Long Expo",
                null,
                Now.AddDays(-2),
                Now.AddDays(5),
                5),
            Event.CreateInstance("Multi-Day Summit 2026",
                "Started yesterday",
                Now.AddHours(-6),
                Now.AddHours(18),
                6),

            // Мероприятие, начинающееся сейчас
            Event.CreateInstance("Starting Now",
                "Edge: starts at Now",
                Now,
                Now.AddHours(4),
                7),

            // Будущие мероприятия (StartAt > Now)
            Event.CreateInstance("Tomorrow Talk",
                null,
                Now.AddDays(1),
                Now.AddDays(1).AddHours(3),
                8),
            Event.CreateInstance("Upcoming Hackathon",
                "Next weekend",
                Now.AddDays(3),
                Now.AddDays(4),
                9),
            Event.CreateInstance("Tech Symposium",
                null,
                Now.AddDays(7),
                Now.AddDays(9),
                10),
            Event.CreateInstance("Summer Fair 2026",
                "Family friendly",
                Now.AddDays(14),
                Now.AddDays(14).AddHours(8),
                100),
            Event.CreateInstance("Annual Gala",
                null,
                Now.AddDays(30),
                Now.AddDays(30).AddHours(5),
                1000),

            // Сегодняшние мероприятия, 1 в прошлом, 1 в будущем
            Event.CreateInstance("Flash Meetup",
                "1 hour, past",
                Now.AddHours(-2),
                Now.AddHours(-1),
                30),
            Event.CreateInstance("Quick Briefing",
                "1 hour, future",
                Now.AddHours(1),
                Now.AddHours(2),
                40),

            // Заканчивающееся прямо сейчас
            Event.CreateInstance("Ending Now",
                "Edge: ends at Now",
                Now.AddDays(-1),
                Now,
                100)
        };
    }
}