using EventManager.Application.Common.Results;
using EventManager.Application.Services.BookingService;
using EventManager.Domain.Entities;
using EventManager.Domain.Entities.Bookings;
using EventManager.Domain.Entities.Users;
using EventManager.Domain.Exceptions;
using EventManager.Infrastructure.Persistence;
using EventManager.Infrastructure.Persistence.Repositories;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

using Testcontainers.PostgreSql;

namespace EventManager.IntegrationTests.Application.Services.BookingService;

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
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2),
            10);
        var eventRepository = new EventRepository(context);
        await eventRepository.AddEventAsync(testEvent, ct);
        await eventRepository.SaveChangesAsync(ct);
        var testUser = User.CreateInstance("test", "123", UserRole.User);
        var userRepository = new UserRepository(context);
        await userRepository.AddUserAsync(testUser, ct);
        await userRepository.SaveChangesAsync(ct);

        var bookingRepository = new BookingRepository(context);
        var bookingService =
            new BookingServiceImpl(eventRepository, bookingRepository, userRepository,
                NullLogger<BookingServiceImpl>.Instance);

        // Act
        var bookingResult = await bookingService.CreateBookingAsync(testEvent.Id, testUser.Id, ct);

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
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2),
            10);
        var eventRepository = new EventRepository(context);
        await eventRepository.AddEventAsync(testEvent, ct);
        await eventRepository.SaveChangesAsync(ct);
        var testUser = User.CreateInstance("test", "123", UserRole.User);
        var userRepository = new UserRepository(context);
        await userRepository.AddUserAsync(testUser, ct);
        await userRepository.SaveChangesAsync(ct);

        var bookingRepository = new BookingRepository(context);
        var bookingService =
            new BookingServiceImpl(eventRepository, bookingRepository, userRepository,
                NullLogger<BookingServiceImpl>.Instance);

        // Act
        var bookingResult1 = await bookingService.CreateBookingAsync(testEvent.Id, testUser.Id, ct);
        var bookingResult2 = await bookingService.CreateBookingAsync(testEvent.Id, testUser.Id, ct);

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
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2),
            totalSeats);
        var eventRepository = new EventRepository(context);
        await eventRepository.AddEventAsync(testEvent, ct);
        await eventRepository.SaveChangesAsync(ct);
        var testUser = User.CreateInstance("test", "123", UserRole.User);
        var userRepository = new UserRepository(context);
        await userRepository.AddUserAsync(testUser, ct);
        await userRepository.SaveChangesAsync(ct);

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
                    var taskUserRepository = new UserRepository(taskContext);
                    var bookingService =
                        new BookingServiceImpl(taskEventRepository, taskBookingRepository, taskUserRepository,
                            NullLogger<BookingServiceImpl>.Instance);

                    return await bookingService.CreateBookingAsync(testEvent.Id, testUser.Id, ct);
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

    [Fact]
    public async Task CancelBookingAsync_WhenBookingAndUserExists_ShouldCancelBooking()
    {
        var ct = TestContext.Current.CancellationToken;
        await ResetDatabaseAsync();

        // Arrange
        const int totalSeats = 10;
        await using var context = CreateContext();
        var testEvent = Event.CreateInstance(
            "Test Event",
            "Description",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2),
            totalSeats);
        var eventRepository = new EventRepository(context);
        await eventRepository.AddEventAsync(testEvent, ct);
        await eventRepository.SaveChangesAsync(ct);
        var testUser = User.CreateInstance("test", "123", UserRole.User);
        var userRepository = new UserRepository(context);
        await userRepository.AddUserAsync(testUser, ct);
        await userRepository.SaveChangesAsync(ct);

        var bookingRepository = new BookingRepository(context);
        var bookingService =
            new BookingServiceImpl(eventRepository, bookingRepository, userRepository,
                NullLogger<BookingServiceImpl>.Instance);

        // Act
        var booking = (await bookingService.CreateBookingAsync(testEvent.Id, testUser.Id, ct)).Value!;

        var result =
            await bookingService.CancelBookingAsync(booking.Id, testUser.Id,
                TestContext.Current.CancellationToken);

        // Assert
        await using var verifyContext = CreateContext();
        var verifyEvent = verifyContext.Events.First(e => e.Id == testEvent.Id);
        var verifyBooking = verifyContext.Bookings.First(b => b.Id == booking.Id);
        result.IsSuccess.Should().BeTrue();

        verifyEvent.AvailableSeats.Should().Be(verifyEvent.TotalSeats);
        verifyBooking.Status.Should().Be(BookingStatus.Cancelled);
    }

    [Fact]
    public async Task CreateBookingAsync_WhenOtherUserHasMaxActiveBookingCount_ShouldReturnBooking()
    {
        var ct = TestContext.Current.CancellationToken;
        await ResetDatabaseAsync();

        // Arrange
        const int totalSeats = BookingConstants.MaxActiveBookingCountPerUser * 2;
        await using var context = CreateContext();
        var testEvent = Event.CreateInstance(
            "Test Event",
            "Description",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2),
            totalSeats);
        var eventRepository = new EventRepository(context);
        await eventRepository.AddEventAsync(testEvent, ct);
        await eventRepository.SaveChangesAsync(ct);
        var testUser1 = User.CreateInstance("test1", "123", UserRole.User);
        var testUser2 = User.CreateInstance("test2", "123", UserRole.User);
        var userRepository = new UserRepository(context);
        await userRepository.AddUserAsync(testUser1, ct);
        await userRepository.AddUserAsync(testUser2, ct);
        await userRepository.SaveChangesAsync(ct);

        var bookingRepository = new BookingRepository(context);
        var bookingService =
            new BookingServiceImpl(eventRepository, bookingRepository, userRepository,
                NullLogger<BookingServiceImpl>.Instance);

        for (var i = 0; i < totalSeats / 2; i++)
        {
            await bookingService.CreateBookingAsync(testEvent.Id, testUser1.Id, ct);
        }

        Func<Task> act = () =>
            bookingService.CreateBookingAsync(testEvent.Id, testUser2.Id, TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync<TooManyActiveBookingsException>();
    }
}