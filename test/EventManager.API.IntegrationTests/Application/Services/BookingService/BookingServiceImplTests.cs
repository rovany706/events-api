using EventManager.API.Application.Services.BookingService;
using EventManager.API.Domain.DataAccess;
using EventManager.API.Domain.Repositories;
using EventManager.API.Models.Entities;
using EventManager.API.Models.Results;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.PostgreSql;

namespace EventManager.API.IntegrationTests.Application.Services.BookingService;

public class BookingServiceImplTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("test_db")
        .Build();
    
    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
    
    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        return new AppDbContext(options);
    }

    private async Task ResetDatabaseAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
    }
    
    [Fact]
    public async Task CreateBookingAsync_WhenEventExists_ShouldReturnNewBookingId()
    {
        var ct = TestContext.Current.CancellationToken;
        await ResetDatabaseAsync();

        // Arrange
        const int expectedBookingId = 1;
        await using var context = CreateContext();
        var testEvent = Event.CreateInstance(
            "Test Event",
            "Description",
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 0, 1, 2), DateTimeKind.Utc),
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 1, 2, 3), DateTimeKind.Utc),
            10);
        var eventRepository = new EventRepository(context);
        await eventRepository.AddEventAsync(testEvent, ct);
        await eventRepository.SaveChangesAsync(ct);

        var bookingRepository = new BookingRepository(context);
        var bookingService =
            new BookingServiceImpl(eventRepository, bookingRepository, NullLogger<BookingServiceImpl>.Instance);
        
        // Act
        var bookingResult = await bookingService.CreateBookingAsync(testEvent.Id, ct);
        
        // Assert
        bookingResult.IsSuccess.Should().BeTrue();
        var actualBookingId = bookingResult.Value!.Id;
        actualBookingId.Should().Be(expectedBookingId);
    }
    
    [Fact]
    public async Task CreateBookingAsync_WhenEventHasBookings_ShouldReturnUniqueBookingId()
    {
        var ct = TestContext.Current.CancellationToken;
        await ResetDatabaseAsync();
        
        // Arrange
        const int expectedBookingId1 = 1;
        const int expectedBookingId2 = 2;
        await using var context = CreateContext();
        var testEvent = Event.CreateInstance(
            "Test Event",
            "Description",
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 0, 1, 2), DateTimeKind.Utc),
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 1, 2, 3), DateTimeKind.Utc),
            10);
        var eventRepository = new EventRepository(context);
        await eventRepository.AddEventAsync(testEvent, ct);
        await eventRepository.SaveChangesAsync(ct);
        
        var bookingRepository = new BookingRepository(context);
        var bookingService =
            new BookingServiceImpl(eventRepository, bookingRepository, NullLogger<BookingServiceImpl>.Instance);

        // Act
        var bookingResult1 = await bookingService.CreateBookingAsync(testEvent.Id, ct);
        var bookingResult2 = await bookingService.CreateBookingAsync(testEvent.Id, ct);
    
        // Assert
        bookingResult1.IsSuccess.Should().BeTrue();
        bookingResult2.IsSuccess.Should().BeTrue();
        var actualBookingId1 = bookingResult1.Value!.Id;
        var actualBookingId2 = bookingResult2.Value!.Id;
        actualBookingId1.Should().Be(expectedBookingId1);
        actualBookingId2.Should().Be(expectedBookingId2);
    }
    
    [Fact]
    public async Task CreateBookingAsync_WhenConcurrent_ShouldReturnUniqueIds()
    {
        var ct = TestContext.Current.CancellationToken;
        await ResetDatabaseAsync();
        
        // Arrange
        const int totalSeats = 10;
        await using var context = CreateContext();
        var testEvent = Event.CreateInstance(
            "Test Event",
            "Description",
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 0, 1, 2), DateTimeKind.Utc),
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 1, 2, 3), DateTimeKind.Utc),
            totalSeats);
        var eventRepository = new EventRepository(context);
        await eventRepository.AddEventAsync(testEvent, ct);
        await eventRepository.SaveChangesAsync(ct);
        
        // Act
        var tasks = new Task<Result<Booking?>>[totalSeats];

        for (var i = 0; i < totalSeats; i++)
        {
            tasks[i] = Task.Run(
                async () =>
                {
                    await using var taskContext = CreateContext();
                    var taskEventRepository = new EventRepository(taskContext);
                    var taskBookingRepository = new BookingRepository(taskContext);
                    var bookingService =
                        new BookingServiceImpl(taskEventRepository, taskBookingRepository, NullLogger<BookingServiceImpl>.Instance);

                    return await bookingService.CreateBookingAsync(testEvent.Id, ct);
                }, 
                ct);
        }

        var bookingResults = await Task.WhenAll(tasks);

        // Assert
        await using var verifyContext = CreateContext();
        var bookedEvent = await verifyContext.Events.FirstAsync(e => e.Id == testEvent.Id, ct);
        bookingResults.All(x => x.IsSuccess).Should().BeTrue();
        bookingResults.Select(x => x.Value!.Id).Should().BeEquivalentTo(Enumerable.Range(1, totalSeats));
        bookedEvent.AvailableSeats.Should().Be(0);
    }
}