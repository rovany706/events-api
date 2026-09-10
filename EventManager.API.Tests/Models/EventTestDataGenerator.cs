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
            Event.CreateInstance(
                1,
                "Past Conference",
                "Already over",
                Now.AddDays(-10),
                Now.AddDays(-8),
                1),
            Event.CreateInstance(
                2,
                "Old Conference",
                null,
                Now.AddDays(-5),
                Now.AddDays(-4),
                2),
            Event.CreateInstance(
                3,
                "Last Week Meetup",
                "Ended recently",
                Now.AddDays(-3),
                Now.AddDays(-2),
                3),

            // Начавшиеся мероприятия (StartAt <= Now < EndAt)
            Event.CreateInstance(
                4,
                "Running Festival",
                "Ongoing right Now",
                Now.AddDays(-1),
                Now.AddDays(1),
                4),
            Event.CreateInstance(
                5,
                "Week-Long Expo",
                null,
                Now.AddDays(-2),
                Now.AddDays(5),
                5),
            Event.CreateInstance(
                6,
                "Multi-Day Summit 2026",
                "Started yesterday",
                Now.AddHours(-6),
                Now.AddHours(18),
                6),

            // Мероприятие, начинающееся сейчас
            Event.CreateInstance(
                7,
                "Starting Now",
                "Edge: starts at Now",
                Now,
                Now.AddHours(4),
                7),

            // Будущие мероприятия (StartAt > Now)
            Event.CreateInstance(
                8,
                "Tomorrow Talk",
                null,
                Now.AddDays(1),
                Now.AddDays(1).AddHours(3),
                8),
            Event.CreateInstance(
                9,
                "Upcoming Hackathon",
                "Next weekend",
                Now.AddDays(3),
                Now.AddDays(4),
                9),
            Event.CreateInstance(
                10,
                "Tech Symposium",
                null,
                Now.AddDays(7),
                Now.AddDays(9),
                10),
            Event.CreateInstance(
                11,
                "Summer Fair 2026",
                "Family friendly",
                Now.AddDays(14),
                Now.AddDays(14).AddHours(8),
                100),
            Event.CreateInstance(
                12,
                "Annual Gala",
                null,
                Now.AddDays(30),
                Now.AddDays(30).AddHours(5),
                1000),

            // Сегодняшние мероприятия, 1 в прошлом, 1 в будущем
            Event.CreateInstance(
                13,
                "Flash Meetup",
                "1 hour, past",
                Now.AddHours(-2),
                Now.AddHours(-1),
                30),
            Event.CreateInstance(
                14,
                "Quick Briefing",
                "1 hour, future",
                Now.AddHours(1),
                Now.AddHours(2),
                40),

            // Заканчивающееся прямо сейчас
            Event.CreateInstance(
                15,
                "Ending Now",
                "Edge: ends at Now",
                Now.AddDays(-1),
                Now,
                100)
        };
    }
}